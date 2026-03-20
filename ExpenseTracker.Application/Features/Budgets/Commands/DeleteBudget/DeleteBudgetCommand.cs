using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.DeleteBudget;

public record DeleteBudgetCommand(Guid Id) : IRequest;
