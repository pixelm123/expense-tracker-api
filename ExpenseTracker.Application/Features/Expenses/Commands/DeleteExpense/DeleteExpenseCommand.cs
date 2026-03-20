using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public record DeleteExpenseCommand(Guid Id) : IRequest;
