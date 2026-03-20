using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Budgets.Commands.CreateBudget;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Budgets;

public class CreateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _budgets = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly CreateBudgetCommandHandler _handler;

    private const string CurrentUserId = "user-1";

    public CreateBudgetCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(CurrentUserId);
        _handler = new CreateBudgetCommandHandler(
            _budgets.Object, _categories.Object, _uow.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new CreateBudgetCommand(Guid.NewGuid(), 1000m, 3, 2025);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenCategoryBelongsToAnotherUser()
    {
        var category = new Category("Food", "other-user");
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var command = new CreateBudgetCommand(category.Id, 1000m, 3, 2025);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldAddBudgetWithCorrectFields_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        Budget? captured = null;
        _budgets.Setup(r => r.Add(It.IsAny<Budget>()))
            .Callback<Budget>(b => captured = b);

        var command = new CreateBudgetCommand(category.Id, 1500m, 3, 2025, AlertThresholdPercentage: 75m);

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(CurrentUserId);
        captured.CategoryId.Should().Be(category.Id);
        captured.LimitAmount.Should().Be(1500m);
        captured.Month.Should().Be(3);
        captured.Year.Should().Be(2025);
        captured.AlertThresholdPercentage.Should().Be(75m);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges_WhenValid()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _handler.Handle(new CreateBudgetCommand(category.Id, 1000m, 3, 2025), CancellationToken.None);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultAlertThreshold_WhenNotSpecified()
    {
        var category = new Category("Food", CurrentUserId);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        Budget? captured = null;
        _budgets.Setup(r => r.Add(It.IsAny<Budget>()))
            .Callback<Budget>(b => captured = b);

        // AlertThresholdPercentage uses the default value (80)
        await _handler.Handle(new CreateBudgetCommand(category.Id, 1000m, 3, 2025), CancellationToken.None);

        captured!.AlertThresholdPercentage.Should().Be(80m);
    }
}
