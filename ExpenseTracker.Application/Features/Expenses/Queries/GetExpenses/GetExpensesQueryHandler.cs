using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, PagedResult<ExpenseDto>>
{
    private readonly IExpenseRepository _expenses;
    private readonly ICurrentUserService _currentUser;

    public GetExpensesQueryHandler(IExpenseRepository expenses, ICurrentUserService currentUser)
    {
        _expenses = expenses;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _expenses.GetPagedAsync(
            _currentUser.UserId,
            request.From,
            request.To,
            request.CategoryId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return PagedResult<ExpenseDto>.Create(
            paged.Items.Select(ExpenseDto.FromEntity).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }
}
