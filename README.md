# ExpenseTracker API

A.NET 10 Web API demonstrating: Clean Architecture, CQRS with MediatR, JWT authentication, background services, cache invalidation, and RFC 7807 error handling.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, ASP.NET Core 10 |
| Architecture | Clean Architecture + CQRS |
| Messaging | MediatR 14 |
| Validation | FluentValidation 12 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 18 (Docker) |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Caching | IMemoryCache |
| Background jobs | .NET BackgroundService |
| Testing | xUnit, Moq, FluentAssertions |

---

## Project Structure

```
ExpenseTracker.sln
├── ExpenseTracker.Domain          # Entities, domain logic, exceptions — zero dependencies
├── ExpenseTracker.Application     # CQRS handlers, validators, repository interfaces
├── ExpenseTracker.Infrastructure  # EF Core, PostgreSQL, Identity, JWT, caching, background service
├── ExpenseTracker.API             # Controllers, middleware, Program.cs
└── ExpenseTracker.Tests           # xUnit unit tests 
```

Dependency flow is strictly one-way: `API → Infrastructure → Application → Domain`. The Domain layer has no NuGet dependencies at all.

---

## Features

- **JWT authentication** — register and login endpoints that return a signed token
- **Expense management** — create, update, delete, and filter by date range and category with pagination
- **Budget alerts** — set a monthly spending limit per category with a configurable alert threshold; a background service checks every hour and logs structured warnings when spending crosses the threshold
- **Monthly summary** — a cached endpoint that returns per-category spending, budget status, and alert flags for any calendar month
- **Cache invalidation on write** — creating, updating, or deleting an expense immediately evicts the affected month's summary from cache
- **RFC 7807 Problem Details** — every error response (400, 403, 404, 500) returns a consistent JSON shape; validation errors include a structured `errors` map by field name

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for PostgreSQL)

### 1. Start PostgreSQL

```bash
docker run -d \
  --name expensesdb \
  -e POSTGRES_DB=expensesdb \
  -e POSTGRES_USER=<your-username> \
  -e POSTGRES_PASSWORD=<your-password> \
  -p 5432:5432 \
  postgres:latest
```

### 2. Apply the database migration

```bash
dotnet ef database update \
  --project ExpenseTracker.Infrastructure \
  --startup-project ExpenseTracker.API
```

### 3. Run the API

```bash
dotnet run --project ExpenseTracker.API
```

The API is available at `https://localhost:7220`. The OpenAPI schema is at `/openapi/v1.json`.

### 4. Run the tests

```bash
dotnet test
```

---

## API Endpoints

All endpoints except `/api/auth/*` require `Authorization: Bearer <token>`.

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register and receive a JWT |
| POST | `/api/auth/login` | Authenticate and receive a JWT |

### Expenses
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/expenses` | Paginated list with optional `from`, `to`, `categoryId` filters |
| GET | `/api/expenses/{id}` | Single expense by ID |
| POST | `/api/expenses` | Create expense |
| PUT | `/api/expenses/{id}` | Update expense |
| DELETE | `/api/expenses/{id}` | Delete expense |

### Categories
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/categories` | All categories for the current user |
| POST | `/api/categories` | Create category |
| PUT | `/api/categories/{id}` | Rename / recolour category |
| DELETE | `/api/categories/{id}` | Delete (fails if expenses are attached) |

### Budgets
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/budgets` | All budgets for the current user |
| POST | `/api/budgets` | Create monthly budget for a category |
| PUT | `/api/budgets/{id}` | Update limit and alert threshold |
| DELETE | `/api/budgets/{id}` | Delete budget |

### Reports
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/reports/monthly-summary?month=3&year=2025` | Cached per-category breakdown |

---

## Error Responses

All errors follow RFC 7807 Problem Details:

```json
{
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/expenses",
  "errors": {
    "Amount": ["Amount must be greater than zero."],
    "CategoryId": ["Category is required."]
  }
}
```

---

## Design Decisions

### 1. CQRS with MediatR rather than a service layer

A traditional service layer (`ExpenseService`, `CategoryService`) tends to grow into a God object that mixes reads and writes, violates SRP, and becomes hard to test in isolation. CQRS makes intent explicit at the type level: a `CreateExpenseCommand` can only do one thing. Each handler is a small, self-contained unit that is trivial to unit-test with mocked dependencies.

The controller stays genuinely thin — it only calls `_sender.Send(command)` and maps the result to an HTTP status code. All logic lives in the handler, where it can be tested without the HTTP stack.

### 2. Domain entities with private setters and a protected constructor

EF Core requires a constructor it can call, but exposing a public parameterless constructor would allow callers to create entities in an invalid state. The pattern used here is a `private` parameterless constructor (EF Core uses reflection to call it) combined with a `public` constructor that enforces all invariants. Properties have `private set`, so mutation is only possible through explicit methods like `expense.Update(...)`.

This means invalid state is *unrepresentable* — you cannot construct an `Expense` with a negative amount or an empty description. Domain exceptions bubble up immediately rather than at persistence time.

### 3. Business logic lives in the entity, not in the handler

The `Budget` entity owns `IsAlertThresholdExceeded`, `IsLimitExceeded`, and `GetSpendingPercentage`. The background service and the monthly summary handler both call these methods directly instead of duplicating the threshold arithmetic. If the business rule changes (e.g. "alert at threshold minus one day's average spend"), it changes in exactly one place and existing tests immediately catch regressions.

### 4. Repository + Unit of Work instead of direct DbContext injection

Injecting `AppDbContext` into handlers works but ties handlers to EF Core. A handler that receives `IExpenseRepository` and `IUnitOfWork` has no knowledge of how data is stored — it could be PostgreSQL today and a document store tomorrow. More practically, it makes tests deterministic: mock repositories return exactly the entities the test prepares, with no database round-trip and no test database to manage.

The `Add` / `Remove` methods on repositories are synchronous (they only mark entities in EF's change tracker); `SaveChangesAsync` on `IUnitOfWork` is the single async flush. This keeps the handler code flat — no `await` clutter on every mutation.

### 5. Cache invalidation on the write side, not the read side

The monthly summary cache is populated lazily on the first `GET /reports/monthly-summary` call and stored for 5 minutes. Rather than relying on time-based expiry alone, every command that changes expense data (`CreateExpense`, `UpdateExpense`, `DeleteExpense`) calls `_cache.Remove(...)` for the affected month(s) immediately after `SaveChangesAsync`. The cache is never stale by more than the duration of the write transaction.

The `UpdateExpense` handler handles the cross-month edge case: if an expense is moved from January to February, it evicts *both* months' cache keys.

### 6. `ICacheService` abstraction instead of `IMemoryCache` directly

Handlers depend on the `ICacheService` interface defined in the Application layer. The `CacheService` implementation in Infrastructure wraps `IMemoryCache`. If the project needs to scale horizontally (multiple API instances cannot share in-process memory), replacing `CacheService` with a Redis-backed implementation requires zero changes to any handler — only the Infrastructure registration changes.

### 7. `ICurrentUserService` instead of `IHttpContextAccessor` in handlers

A handler that reads `HttpContext.User.FindFirst(...)` directly is implicitly coupled to the HTTP stack and harder to test. `ICurrentUserService` is a one-property interface defined in Application. The implementation in Infrastructure reads the `sub` claim from `IHttpContextAccessor`. In tests, the mock is a single line: `_currentUser.Setup(u => u.UserId).Returns("user-1")`.

### 8. `ForbiddenException` (403) instead of `NotFoundException` (404) for ownership violations

When a user requests another user's expense, returning 404 is tempting because it hides whether the resource exists. However, 403 is the correct semantic here: the resource exists but the caller is not permitted to access it. This choice is safe because all endpoints require authentication — there is no risk of confirming a record's existence to an anonymous caller.

### 9. Background service uses `IServiceScopeFactory`, not direct DbContext injection

`BackgroundService` is a Singleton because it lives for the lifetime of the application. `AppDbContext` is Scoped — it must not be consumed directly from a Singleton, as this creates a captured-scope bug where the same `DbContext` is reused across ticks. `IServiceScopeFactory` is Singleton-safe; the background service creates a fresh scope on each tick, gets a new `DbContext` from that scope, and disposes it when the tick completes.

The service also fetches all active budgets and all relevant spending in two queries rather than issuing one query per budget, avoiding an N+1 pattern at scale.

### 10. Schema constraints mirror the business rules

Constraints that live only in application code can be bypassed by any tool that writes to the database directly. Critical rules are enforced at the schema level:

- A user cannot have two categories with the same name — `UNIQUE(UserId, Name)`
- A user cannot have two budgets for the same category in the same month — `UNIQUE(UserId, CategoryId, Month, Year)`
- `DeleteBehavior.Restrict` on Category → Expenses prevents silent cascade-deletes; the API must handle and communicate the constraint to the caller explicitly

---

## Running Tests

```bash
dotnet test --logger "console;verbosity=normal"
```


Tests are organised by layer and by feature. Handler tests mock all dependencies and assert on behaviour, not implementation details. Notable tests:

- **Cache hit skips all database calls** — verified with `Times.Never` on repository mocks
- **Cross-month cache invalidation** — moving an expense from January to February must evict both months
- **Budget threshold vs limit distinction** — threshold-exceeded and limit-exceeded are separate assertions because they are separate business states
- **`ForbiddenException` vs `NotFoundException` for ownership checks** — both access paths are tested independently for every mutation handler

