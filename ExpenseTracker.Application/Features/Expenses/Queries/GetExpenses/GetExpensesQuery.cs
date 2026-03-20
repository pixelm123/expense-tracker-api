using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(
    DateTime? From,
    DateTime? To,
    Guid? CategoryId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ExpenseDto>>;
