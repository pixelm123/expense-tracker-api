using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _db;

    public ExpenseRepository(AppDbContext db) => _db = db;

    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedResult<Expense>> GetPagedAsync(
        string userId,
        DateTime? from,
        DateTime? to,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value.ToUniversalTime());

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value.ToUniversalTime());

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Expense>.Create(items, totalCount, page, pageSize);
    }

    public async Task<decimal> GetMonthlyTotalAsync(
        string userId,
        Guid categoryId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = MonthBounds(month, year);

        return await _db.Expenses
            .Where(e => e.UserId == userId
                     && e.CategoryId == categoryId
                     && e.Date >= start
                     && e.Date < end)
            .SumAsync(e => e.Amount, cancellationToken);
    }

    public async Task<Dictionary<Guid, decimal>> GetMonthlyTotalsByCategoryAsync(
        string userId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = MonthBounds(month, year);

        return await _db.Expenses
            .Where(e => e.UserId == userId && e.Date >= start && e.Date < end)
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total, cancellationToken);
    }

    public void Add(Expense expense) => _db.Expenses.Add(expense);

    public void Remove(Expense expense) => _db.Expenses.Remove(expense);

    private static (DateTime Start, DateTime End) MonthBounds(int month, int year)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(1));
    }
}
