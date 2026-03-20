using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand>
{
    private readonly IExpenseRepository _expenses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public DeleteExpenseCommandHandler(
        IExpenseRepository expenses,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _expenses = expenses;
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _expenses.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (expense.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        _expenses.Remove(expense);
        await _uow.SaveChangesAsync(cancellationToken);

        _cache.Remove($"monthly-summary:{expense.Date.Month}:{expense.Date.Year}");
    }
}
