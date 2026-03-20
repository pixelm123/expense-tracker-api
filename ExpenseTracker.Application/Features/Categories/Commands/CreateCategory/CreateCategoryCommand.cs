using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string? Color) : IRequest<Guid>;
