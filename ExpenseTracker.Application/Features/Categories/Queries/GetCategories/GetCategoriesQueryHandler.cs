using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categories.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        return categories.Select(CategoryDto.FromEntity).ToList();
    }
}
