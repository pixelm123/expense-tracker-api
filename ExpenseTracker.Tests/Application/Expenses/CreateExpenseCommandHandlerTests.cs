using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Expenses;

public class CreateExpenseCommandHandlerTests
{
    private readonly Mock<IExpenseRepository> _expenses = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly CreateExpenseCommandHandler _handler;

    private const string CurrentUserId = "user-1";

    public CreateExpenseCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(CurrentUserId);
        _handler = new CreateExpenseCommandHandler(
            _expenses.Object, _categories.Object, _uow.Object,
            _currentUser.Object, _cache.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new CreateExpenseCommand(50m, "Lunch", DateTime.UtcNow.AddDays(-1), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenCategoryBelongsToAnotherUser()
    {
        var category = new Category("Food", "other-user"); // different user owns this category
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new CreateExpenseCommand(50m, "Lunch", DateTime.UtcNow.AddDays(-1), category.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldAddExpense_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        Expense? captured = null;
        _expenses.Setup(r => r.Add(It.IsAny<Expense>()))
            .Callback<Expense>(e => captured = e);

        var command = new CreateExpenseCommand(49.99m, "Coffee", DateTime.UtcNow.AddDays(-1), category.Id);

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Amount.Should().Be(49.99m);
        captured.Description.Should().Be("Coffee");
        captured.UserId.Should().Be(CurrentUserId);
        captured.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new CreateExpenseCommand(50m, "Lunch", DateTime.UtcNow.AddDays(-1), category.Id);

        await _handler.Handle(command, CancellationToken.None);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateMonthlySummaryCache_WhenExpenseCreated()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var expenseDate = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var command = new CreateExpenseCommand(50m, "Lunch", expenseDate, category.Id);

        await _handler.Handle(command, CancellationToken.None);

        _cache.Verify(c => c.Remove("monthly-summary:3:2025"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewExpenseId_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new CreateExpenseCommand(50m, "Lunch", DateTime.UtcNow.AddDays(-1), category.Id);

        var id = await _handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
    }
}
