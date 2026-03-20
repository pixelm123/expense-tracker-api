using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Expense>> GetPagedAsync(
        string userId,
        DateTime? from,
        DateTime? to,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<decimal> GetMonthlyTotalAsync(
        string userId,
        Guid categoryId,
        int month,
        int year,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, decimal>> GetMonthlyTotalsByCategoryAsync(
        string userId,
        int month,
        int year,
        CancellationToken cancellationToken = default);

    void Add(Expense expense);
    void Remove(Expense expense);
}
