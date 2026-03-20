using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto?>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
        => _authService = authService;

    public Task<AuthResponseDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
        => _authService.LoginAsync(request.Email, request.Password, cancellationToken);
}
