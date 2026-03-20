using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.LimitAmount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(b => b.AlertThresholdPercentage).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Month).IsRequired();
        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year }).IsUnique();
    }
}
