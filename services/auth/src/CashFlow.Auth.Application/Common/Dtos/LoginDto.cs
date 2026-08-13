namespace CashFlow.Auth.Application.Common.Dtos;

public sealed record LoginDto(string Token,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc);

