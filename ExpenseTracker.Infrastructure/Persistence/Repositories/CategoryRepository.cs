using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Category>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => _db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public void Add(Category category) => _db.Categories.Add(category);

    public void Remove(Category category) => _db.Categories.Remove(category);
}
