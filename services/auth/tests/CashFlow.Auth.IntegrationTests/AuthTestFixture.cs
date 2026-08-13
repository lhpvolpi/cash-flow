using ApplicationDependencyInjection = CashFlow.Auth.Application.DependencyInjection;
using InfrastructureDependencyInjection = CashFlow.Auth.Infrastructure.DependencyInjection;

namespace CashFlow.Auth.IntegrationTests;

public sealed class AuthTestFixture : IDisposable
{
    private readonly ServiceProvider _services;

    public const string ValidUsername = "admin";
    public const string ValidPassword = "admin123";
    public const string JwtIssuer = "CashFlow.Auth";
    public const string JwtAudience = "CashFlow";

    public IServiceProvider Services => _services;

    public AuthTestFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-key-not-for-production-use-1234567890",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:ExpirationMinutes"] = "60",
                ["Auth:Username"] = ValidUsername,
                ["Auth:Password"] = ValidPassword
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        ApplicationDependencyInjection.AddApplicationServices(services);
        InfrastructureDependencyInjection.AddInfrastructureServices(services, configuration);

        _services = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
