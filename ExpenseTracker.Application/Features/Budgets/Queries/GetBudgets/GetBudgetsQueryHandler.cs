using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, List<BudgetDto>>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetsQueryHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _currentUser = currentUser;
    }

    public async Task<List<BudgetDto>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var budgets = await _budgets.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        return budgets.Select(BudgetDto.FromEntity).ToList();
    }
}
