using CashFlow.Auth.Application.Abstractions;
using CashFlow.Auth.Application.Common.Dtos;

namespace CashFlow.Auth.Application.AuthTokens.Login.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginDto>
{
    private readonly IAuthTokenService _authTokenService;

    public LoginCommandHandler(IAuthTokenService authTokenService)
    {
        _authTokenService = authTokenService;
    }

    public Task<LoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authToken = _authTokenService.GetAuthToken(request.Username, request.Password);

        return Task.FromResult(new LoginDto(authToken.Token, authToken.RefreshToken, authToken.ExpiresAtUtc));
    }
}

