using CashFlow.Auth.Application.Abstractions;
using CashFlow.Auth.Infrastructure.Services;

namespace CashFlow.Auth.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthTokenService, AuthTokenService>();
    }
}
