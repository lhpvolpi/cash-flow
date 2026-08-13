using CashFlow.Auth.Application.Abstractions;
using CashFlow.Auth.Application.AuthTokens.Login.Commands;
using CashFlow.Auth.Application.Common.Exceptions;
using CashFlow.Auth.Domain.Entities;

namespace CashFlow.Auth.Application.Tests;

public class LoginCommandHandlerTests
{
    private readonly IAuthTokenService _authTokenService = Substitute.For<IAuthTokenService>();

    private LoginCommandHandler CreateHandler() => new(_authTokenService);

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsLoginDtoFromAuthToken()
    {
        // Arrange
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1);
        var authToken = new AuthToken("jwt-token", "refresh-token", expiresAtUtc);

        _authTokenService
            .GetAuthToken("admin", "admin123")
            .Returns(authToken);

        var handler = CreateHandler();
        var command = new LoginCommand("admin", "admin123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(expiresAtUtc, result.ExpiresAtUtc);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsInvalidCredentials_PropagatesException()
    {
        // Arrange
        _authTokenService
            .GetAuthToken("admin", "wrong-password")
            .Throws(new InvalidCredentialsException());

        var handler = CreateHandler();
        var command = new LoginCommand("admin", "wrong-password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}
