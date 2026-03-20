using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseTracker.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("User is not authenticated.");
}
