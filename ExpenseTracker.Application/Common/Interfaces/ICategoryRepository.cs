using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    void Add(Category category);
    void Remove(Category category);
}
