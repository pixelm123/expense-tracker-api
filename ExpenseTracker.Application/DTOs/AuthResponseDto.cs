namespace ExpenseTracker.Application.DTOs;

public record AuthResponseDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt);
