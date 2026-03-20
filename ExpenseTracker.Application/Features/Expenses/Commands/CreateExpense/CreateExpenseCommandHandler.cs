using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Guid>
{
    private readonly IExpenseRepository _expenses;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateExpenseCommandHandler(
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

    public async Task<Guid> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (category.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        var expense = new Expense(
            _currentUser.UserId,
            request.CategoryId,
            request.Amount,
            request.Description,
            request.Date);

        _expenses.Add(expense);
        await _uow.SaveChangesAsync(cancellationToken);

        _cache.Remove(MonthlySummaryCacheKey(expense.Date.Month, expense.Date.Year));

        return expense.Id;
    }

    public static string MonthlySummaryCacheKey(int month, int year)
        => $"monthly-summary:{month}:{year}";
}
