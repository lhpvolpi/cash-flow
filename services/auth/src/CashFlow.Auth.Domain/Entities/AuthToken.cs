namespace CashFlow.Auth.Domain.Entities;

public class AuthToken
{
    public AuthToken(string token, string refreshToken, DateTimeOffset expiresAtUtc)
    {
        Token = token;
        RefreshToken = refreshToken;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string Token { get; private set; }

    public string RefreshToken { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }
}
