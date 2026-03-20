using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand>
{
    private readonly IExpenseRepository _expenses;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateExpenseCommandHandler(
        IExpenseRepository expenses,
        ICategoryRepository categories,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _expenses = expenses;
        _categories = categories;
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _expenses.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (expense.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (category.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        var oldMonth = expense.Date.Month;
        var oldYear = expense.Date.Year;

        expense.Update(request.Amount, request.Description, request.Date, request.CategoryId);
        await _uow.SaveChangesAsync(cancellationToken);

        _cache.Remove(MonthlySummaryCacheKey(oldMonth, oldYear));
        _cache.Remove(MonthlySummaryCacheKey(expense.Date.Month, expense.Date.Year));
    }

    private static string MonthlySummaryCacheKey(int month, int year)
        => $"monthly-summary:{month}:{year}";
}
