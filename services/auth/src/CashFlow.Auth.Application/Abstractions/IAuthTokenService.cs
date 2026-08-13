using CashFlow.Auth.Domain.Entities;

namespace CashFlow.Auth.Application.Abstractions;

public interface IAuthTokenService
{
    AuthToken GetAuthToken(string username, string password);
}

