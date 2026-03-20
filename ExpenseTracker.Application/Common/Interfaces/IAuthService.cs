using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    Task<AuthResponseDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
