using CashFlow.Auth.Application.AuthTokens.Login.Commands;
using CashFlow.Auth.Application.Common.Exceptions;
using CashFlow.Shared.Application.Common;

namespace CashFlow.Auth.IntegrationTests;

public class LoginFlowTests : IClassFixture<AuthTestFixture>
{
    private readonly AuthTestFixture _fixture;

    public LoginFlowTests(AuthTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAWellFormedJwt()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new LoginCommand(AuthTestFixture.ValidUsername, AuthTestFixture.ValidPassword);

        // Act
        var result = await mediator.Send(command, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Equal(AuthTestFixture.JwtIssuer, jwt.Issuer);
        Assert.Contains(AuthTestFixture.JwtAudience, jwt.Audiences);
        Assert.Equal(AuthTestFixture.ValidUsername, jwt.Subject);
        Assert.Equal(result.ExpiresAtUtc, jwt.ValidTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new LoginCommand(AuthTestFixture.ValidUsername, "wrong-password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => mediator.Send(command, CancellationToken.None));
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ThrowsValidationException()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new LoginCommand("", "");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => mediator.Send(command, CancellationToken.None));
    }
}
