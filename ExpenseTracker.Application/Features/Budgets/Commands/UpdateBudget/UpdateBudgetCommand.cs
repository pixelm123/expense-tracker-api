using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.UpdateBudget;

public record UpdateBudgetCommand(
    Guid Id,
    decimal LimitAmount,
    decimal AlertThresholdPercentage) : IRequest;
