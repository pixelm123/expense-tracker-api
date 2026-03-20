using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Commands.DeleteBudget;

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand>
{
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteBudgetCommandHandler(
        IBudgetRepository budgets,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgets.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Budget), request.Id);

        if (budget.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        _budgets.Remove(budget);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
