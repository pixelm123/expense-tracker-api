using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Queries.GetBudgets;

public record GetBudgetsQuery : IRequest<List<BudgetDto>>;
