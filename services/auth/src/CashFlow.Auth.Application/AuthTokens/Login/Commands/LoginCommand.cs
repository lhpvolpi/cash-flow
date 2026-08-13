using CashFlow.Auth.Application.Common.Dtos;

namespace CashFlow.Auth.Application.AuthTokens.Login.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginDto>;

