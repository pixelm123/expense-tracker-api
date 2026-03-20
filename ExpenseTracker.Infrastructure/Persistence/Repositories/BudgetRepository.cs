using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;

    public BudgetRepository(AppDbContext db) => _db = db;

    public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<Budget>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => _db.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ToListAsync(cancellationToken);

    public Task<List<Budget>> GetByMonthYearAsync(string userId, int month, int year, CancellationToken cancellationToken = default)
        => _db.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .ToListAsync(cancellationToken);

    public void Add(Budget budget) => _db.Budgets.Add(budget);

    public void Remove(Budget budget) => _db.Budgets.Remove(budget);
}
