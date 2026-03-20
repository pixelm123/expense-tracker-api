using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.DTOs;

public record ExpenseDto(
    Guid Id,
    decimal Amount,
    string Description,
    DateTime Date,
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ExpenseDto FromEntity(Expense e)
        => new(
            e.Id,
            e.Amount,
            e.Description,
            e.Date,
            e.CategoryId,
            e.Category?.Name ?? string.Empty,
            e.Category?.Color,
            e.CreatedAt,
            e.UpdatedAt);
}
