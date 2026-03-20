using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand>
{
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateBudgetCommandHandler(
        IBudgetRepository budgets,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgets.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Budget), request.Id);

        if (budget.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        budget.Update(request.LimitAmount, request.AlertThresholdPercentage);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
