using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Expenses;

public class UpdateExpenseCommandHandlerTests
{
    private readonly Mock<IExpenseRepository> _expenses = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly UpdateExpenseCommandHandler _handler;

    private const string CurrentUserId = "user-1";

    public UpdateExpenseCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(CurrentUserId);
        _handler = new UpdateExpenseCommandHandler(
            _expenses.Object, _categories.Object, _uow.Object,
            _currentUser.Object, _cache.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenExpenseDoesNotExist()
    {
        _expenses.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var command = new UpdateExpenseCommand(Guid.NewGuid(), 50m, "Lunch", DateTime.UtcNow, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenExpenseBelongsToAnotherUser()
    {
        var expense = new Expense("other-user", Guid.NewGuid(), 50m, "Lunch", DateTime.UtcNow.AddDays(-1));
        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var command = new UpdateExpenseCommand(expense.Id, 60m, "Dinner", DateTime.UtcNow.AddDays(-1), expense.CategoryId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenNewCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var expense = new Expense(CurrentUserId, categoryId, 50m, "Lunch", DateTime.UtcNow.AddDays(-1));
        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new UpdateExpenseCommand(expense.Id, 60m, "Dinner", DateTime.UtcNow.AddDays(-1), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenNewCategoryBelongsToAnotherUser()
    {
        var expense = new Expense(CurrentUserId, Guid.NewGuid(), 50m, "Lunch", DateTime.UtcNow.AddDays(-1));
        var foreignCategory = new Category("Entertainment", "other-user");

        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        _categories.Setup(r => r.GetByIdAsync(foreignCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foreignCategory);

        var command = new UpdateExpenseCommand(expense.Id, 60m, "Movie", DateTime.UtcNow.AddDays(-1), foreignCategory.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateExpenseAndSave_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        var expense = new Expense(CurrentUserId, category.Id, 50m, "Lunch", DateTime.UtcNow.AddDays(-1));

        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new UpdateExpenseCommand(expense.Id, 99m, "Dinner", DateTime.UtcNow.AddDays(-1), category.Id);

        await _handler.Handle(command, CancellationToken.None);

        expense.Amount.Should().Be(99m);
        expense.Description.Should().Be("Dinner");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateBothMonthCaches_WhenDateChangesAcrossMonths()
    {
        var category = new Category("Food", CurrentUserId);
        var oldDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var newDate = new DateTime(2025, 2, 20, 0, 0, 0, DateTimeKind.Utc);

        var expense = new Expense(CurrentUserId, category.Id, 50m, "Lunch", oldDate);

        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new UpdateExpenseCommand(expense.Id, 60m, "Dinner", newDate, category.Id);

        await _handler.Handle(command, CancellationToken.None);

        // Both the old month (January) and the new month (February) must be evicted.
        _cache.Verify(c => c.Remove("monthly-summary:1:2025"), Times.Once);
        _cache.Verify(c => c.Remove("monthly-summary:2:2025"), Times.Once);
    }
}
