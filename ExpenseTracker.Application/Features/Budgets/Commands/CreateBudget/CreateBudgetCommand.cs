using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.CreateBudget;

public record CreateBudgetCommand(
    Guid CategoryId,
    decimal LimitAmount,
    int Month,
    int Year,
    decimal AlertThresholdPercentage = 80) : IRequest<Guid>;
