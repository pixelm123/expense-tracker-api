using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return null;

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto(
            UserId: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Token: token,
            ExpiresAt: expiresAt);
    }

    public async Task<AuthResponseDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            return null;

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto(
            UserId: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Token: token,
            ExpiresAt: expiresAt);
    }
}
