using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, Guid>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateBudgetCommandHandler(
        IBudgetRepository budgets,
        ICategoryRepository categories,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _categories = categories;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (category.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        var budget = new Budget(
            _currentUser.UserId,
            request.CategoryId,
            request.LimitAmount,
            request.Month,
            request.Year,
            request.AlertThresholdPercentage);

        _budgets.Add(budget);
        await _uow.SaveChangesAsync(cancellationToken);
        return budget.Id;
    }
}
