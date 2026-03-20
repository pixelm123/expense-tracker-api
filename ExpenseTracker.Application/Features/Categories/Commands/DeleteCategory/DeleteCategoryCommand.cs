using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest;
