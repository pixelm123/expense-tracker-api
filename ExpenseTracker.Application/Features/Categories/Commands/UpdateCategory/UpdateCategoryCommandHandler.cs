using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _categories = categories;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        if (category.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        category.Update(request.Name, request.Color);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
