using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public record UpdateExpenseCommand(
    Guid Id,
    decimal Amount,
    string Description,
    DateTime Date,
    Guid CategoryId) : IRequest;
