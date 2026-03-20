using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public string UserId { get; private set; } = string.Empty;

    private readonly List<Expense> _expenses = [];
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private readonly List<Budget> _budgets = [];
    public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();

    private Category() { }

    public Category(string name, string userId, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId is required.");

        Name = name.Trim();
        UserId = userId;
        Color = color?.Trim();
    }

    public void Update(string name, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        Name = name.Trim();
        Color = color?.Trim();
        Touch();
    }
}
