using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenseById;

public record GetExpenseByIdQuery(Guid Id) : IRequest<ExpenseDto>;
