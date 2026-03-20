using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.Features.Categories.Commands.CreateCategory;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly CreateCategoryCommandHandler _handler;

    private const string CurrentUserId = "user-1";

    public CreateCategoryCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(CurrentUserId);
        _handler = new CreateCategoryCommandHandler(_categories.Object, _uow.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddCategoryBelongingToCurrentUser_WhenValid()
    {
        Category? captured = null;
        _categories.Setup(r => r.Add(It.IsAny<Category>()))
            .Callback<Category>(c => captured = c);

        await _handler.Handle(new CreateCategoryCommand("Food", "#FF5733"), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(CurrentUserId);
        captured.Name.Should().Be("Food");
        captured.Color.Should().Be("#FF5733");
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges_WhenValid()
    {
        await _handler.Handle(new CreateCategoryCommand("Food", null), CancellationToken.None);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewCategoryId_WhenValid()
    {
        var id = await _handler.Handle(new CreateCategoryCommand("Travel", "#0055FF"), CancellationToken.None);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldAllowNullColor_WhenNotProvided()
    {
        Category? captured = null;
        _categories.Setup(r => r.Add(It.IsAny<Category>()))
            .Callback<Category>(c => captured = c);

        await _handler.Handle(new CreateCategoryCommand("Food", null), CancellationToken.None);

        captured!.Color.Should().BeNull();
    }
}
