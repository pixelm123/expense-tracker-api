using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.DTOs;

public record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    decimal LimitAmount,
    int Month,
    int Year,
    decimal AlertThresholdPercentage,
    DateTime CreatedAt)
{
    public static BudgetDto FromEntity(Budget b)
        => new(
            b.Id,
            b.CategoryId,
            b.Category?.Name ?? string.Empty,
            b.LimitAmount,
            b.Month,
            b.Year,
            b.AlertThresholdPercentage,
            b.CreatedAt);
}
