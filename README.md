# ExpenseTracker API

A .NET 10 Web API demonstrating Clean Architecture, CQRS, JWT auth, background jobs, caching, and RFC 7807 error handling.

## Stack

| | |
|---|---|
| Runtime | .NET 10, ASP.NET Core |
| Pattern | Clean Architecture + CQRS (MediatR) |
| Database | PostgreSQL via EF Core + Npgsql |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation (pipeline behavior) |
| Caching | IMemoryCache |
| Testing | xUnit, Moq, FluentAssertions |

## Getting Started

**Prerequisites:** .NET 10 SDK, Docker

```bash
# Start PostgreSQL
docker run -d --name expensesdb \
  -e POSTGRES_DB=expensesdb -e POSTGRES_USER= -e POSTGRES_PASSWORD= \
  -p 5432:5432 postgres:latest

# Apply migrations
dotnet ef database update --project ExpenseTracker.Infrastructure --startup-project ExpenseTracker.API

# Run
dotnet run --project ExpenseTracker.API --launch-profile http
```

Open **http://localhost:5027/scalar/v1** for the interactive API UI.

## Endpoints

All routes except `/api/auth/*` require `Authorization: Bearer <token>`.

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Register, returns JWT |
| POST | `/api/auth/login` | Login, returns JWT |
| GET | `/api/expenses` | Paginated list — filters: `from`, `to`, `categoryId` |
| GET | `/api/expenses/{id}` | Single expense |
| POST | `/api/expenses` | Create |
| PUT | `/api/expenses/{id}` | Update |
| DELETE | `/api/expenses/{id}` | Delete |
| GET | `/api/categories` | List |
| POST | `/api/categories` | Create |
| PUT | `/api/categories/{id}` | Update |
| DELETE | `/api/categories/{id}` | Delete |
| GET | `/api/budgets` | List |
| POST | `/api/budgets` | Create monthly budget for a category |
| PUT | `/api/budgets/{id}` | Update |
| DELETE | `/api/budgets/{id}` | Delete |
| GET | `/api/reports/monthly-summary` | Cached per-category breakdown — params: `month`, `year` |

## Design Notes

- **CQRS over a service layer** — each handler does one thing and is easy to test in isolation; controllers only call `_sender.Send()`
- **Domain validation in constructors** — entities cannot be created in an invalid state; `private set` means mutation only goes through explicit methods
- **Write-side cache invalidation** — create/update/delete all evict the monthly summary cache immediately; update handles the cross-month edge case by evicting both old and new month
- **`ICurrentUserService` / `ICacheService` interfaces** — keeps handlers testable without HTTP context or a real cache; swap `CacheService` for Redis later without touching a handler
- **Background service uses `IServiceScopeFactory`** — `BackgroundService` is singleton, `DbContext` is scoped; a fresh scope is created per tick to avoid the captured-scope bug; budgets and spending are fetched in two queries rather than N+1
- **403 not 404 for ownership violations** — the resource exists, the caller just isn't allowed; returning 404 here would be misleading
- **Schema constraints** — uniqueness rules (one budget per category per month, unique category names per user) are enforced at the database level, not just in application code
