using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.Domain.Entities;

public class Expense : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }

    private Expense() { }

    public Expense(string userId, Guid categoryId, decimal amount, string description, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId is required.");
        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId is required.");
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Description = description.Trim();
        Date = date.ToUniversalTime();
    }

    public void Update(decimal amount, string description, DateTime date, Guid categoryId)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId is required.");

        Amount = amount;
        Description = description.Trim();
        Date = date.ToUniversalTime();
        CategoryId = categoryId;
        Touch();
    }
}
