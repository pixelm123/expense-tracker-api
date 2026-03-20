using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Expenses;

public class DeleteExpenseCommandHandlerTests
{
    private readonly Mock<IExpenseRepository> _expenses = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly DeleteExpenseCommandHandler _handler;

    private const string CurrentUserId = "user-1";

    public DeleteExpenseCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(CurrentUserId);
        _handler = new DeleteExpenseCommandHandler(
            _expenses.Object, _uow.Object, _currentUser.Object, _cache.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenExpenseDoesNotExist()
    {
        _expenses.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        var act = () => _handler.Handle(new DeleteExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenExpenseBelongsToAnotherUser()
    {
        var expense = new Expense("other-user", Guid.NewGuid(), 50m, "Lunch", DateTime.UtcNow.AddDays(-1));
        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        var act = () => _handler.Handle(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldRemoveExpenseAndSaveChanges_WhenValid()
    {
        var expenseDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var expense = new Expense(CurrentUserId, Guid.NewGuid(), 50m, "Lunch", expenseDate);
        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        await _handler.Handle(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        _expenses.Verify(r => r.Remove(expense), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateMonthlySummaryCache_AfterDeletion()
    {
        var expenseDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var expense = new Expense(CurrentUserId, Guid.NewGuid(), 50m, "Lunch", expenseDate);
        _expenses.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        await _handler.Handle(new DeleteExpenseCommand(expense.Id), CancellationToken.None);

        _cache.Verify(c => c.Remove("monthly-summary:3:2025"), Times.Once);
    }
}
