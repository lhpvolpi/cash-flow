using CashFlow.Auth.Application.Abstractions;
using CashFlow.Auth.Application.Common.Exceptions;
using CashFlow.Auth.Domain.Entities;

namespace CashFlow.Auth.Infrastructure.Services;

public class AuthTokenService : IAuthTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly string _validUsername;
    private readonly string _validPassword;

    public AuthTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret not found in configuration");
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer not found in configuration");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience not found in configuration");
        _expirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;
        _validUsername = configuration["Auth:Username"]
            ?? throw new InvalidOperationException("Auth:Username not found in configuration");
        _validPassword = configuration["Auth:Password"]
            ?? throw new InvalidOperationException("Auth:Password not found in configuration");
    }

    public AuthToken GetAuthToken(string username, string password)
    {
        ValidateCredentials(username, password);

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_expirationMinutes);

        var token = GenerateJwt(username, expiresAtUtc);
        var refreshToken = GenerateRefreshToken();

        return new AuthToken(token, refreshToken, expiresAtUtc);
    }

    private void ValidateCredentials(string username, string password)
    {
        var usernameMatches = string.Equals(username, _validUsername, StringComparison.Ordinal);
        var passwordMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(_validPassword));

        if (!usernameMatches || !passwordMatches)
        {
            throw new InvalidCredentialsException();
        }
    }

    private string GenerateJwt(string username, DateTimeOffset expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
