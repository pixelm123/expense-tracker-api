using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Budget>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Budget>> GetByMonthYearAsync(string userId, int month, int year, CancellationToken cancellationToken = default);
    void Add(Budget budget);
    void Remove(Budget budget);
}
