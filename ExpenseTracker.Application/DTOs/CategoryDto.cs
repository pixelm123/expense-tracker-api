using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Color,
    DateTime CreatedAt)
{
    public static CategoryDto FromEntity(Category c)
        => new(c.Id, c.Name, c.Color, c.CreatedAt);
}
