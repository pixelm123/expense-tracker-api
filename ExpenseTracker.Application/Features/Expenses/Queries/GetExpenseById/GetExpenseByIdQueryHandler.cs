using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Features.Expenses.Queries.GetExpenseById;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
{
    private readonly IExpenseRepository _expenses;
    private readonly ICurrentUserService _currentUser;

    public GetExpenseByIdQueryHandler(IExpenseRepository expenses, ICurrentUserService currentUser)
    {
        _expenses = expenses;
        _currentUser = currentUser;
    }

    public async Task<ExpenseDto> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _expenses.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (expense.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        return ExpenseDto.FromEntity(expense);
    }
}
