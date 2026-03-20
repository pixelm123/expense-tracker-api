using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _categories = categories;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, _currentUser.UserId, request.Color);
        _categories.Add(category);
        await _uow.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
