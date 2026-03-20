using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;

public record CreateExpenseCommand(
    decimal Amount,
    string Description,
    DateTime Date,
    Guid CategoryId) : IRequest<Guid>;
