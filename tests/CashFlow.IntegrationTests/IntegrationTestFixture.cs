using ConsolidationApplicationDependencyInjection = CashFlow.Consolidation.Application.DependencyInjection;
using ConsolidationDbContext = CashFlow.Consolidation.Infrastructure.Data.ApplicationDbContext;
using ConsolidationInfrastructureDependencyInjection = CashFlow.Consolidation.Infrastructure.DependencyInjection;
using TransactionApplicationDependencyInjection = CashFlow.Transactions.Application.DependencyInjection;
using TransactionInfrastructureDependencyInjection = CashFlow.Transactions.Infrastructure.DependencyInjection;
using TransactionsDbContext = CashFlow.Infrastructure.Data.ApplicationDbContext;

namespace CashFlow.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private RabbitMqContainer _rabbitMq = null!;
    private ServiceProvider _transactionsServices = null!;
    private ServiceProvider _consolidationServices = null!;

    public IServiceProvider TransactionsServices => _transactionsServices;
    public IServiceProvider ConsolidationServices => _consolidationServices;
    public string RabbitMqConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-management-alpine")
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());

        await CreateDatabaseAsync("transactions");
        await CreateDatabaseAsync("consolidation");

        RabbitMqConnectionString = _rabbitMq.GetConnectionString();

        _transactionsServices = BuildTransactionsServices(BuildDatabaseConnectionString("transactions"));
        _consolidationServices = BuildConsolidationServices(BuildDatabaseConnectionString("consolidation"), retryDelayMilliseconds: null, maxRetries: null);

        await MigrateAsync<TransactionsDbContext>(_transactionsServices);
        await MigrateAsync<ConsolidationDbContext>(_consolidationServices);
    }

    public async Task DisposeAsync()
    {
        await _transactionsServices.DisposeAsync();
        await _consolidationServices.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public ServiceProvider BuildFastRetryConsolidationServices(int maxRetries, int retryDelayMilliseconds) =>
        BuildConsolidationServices(BuildDatabaseConnectionString("consolidation"), retryDelayMilliseconds, maxRetries);

    private string BuildDatabaseConnectionString(string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = database
        };

        return builder.ConnectionString;
    }

    private async Task CreateDatabaseAsync(string database)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{database}\"";
        await command.ExecuteNonQueryAsync();
    }

    private ServiceProvider BuildTransactionsServices(string dbConnectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = dbConnectionString,
                ["ConnectionStrings:RabbitMq"] = RabbitMqConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSimpleConsole(o => o.SingleLine = true));
        services.AddSingleton<IConfiguration>(configuration);
        TransactionApplicationDependencyInjection.AddApplicationServices(services);
        TransactionInfrastructureDependencyInjection.AddInfrastructureServices(services, configuration);

        return services.BuildServiceProvider();
    }

    private ServiceProvider BuildConsolidationServices(string dbConnectionString, int? retryDelayMilliseconds, int? maxRetries)
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = dbConnectionString,
            ["ConnectionStrings:RabbitMq"] = RabbitMqConnectionString
        };

        if (maxRetries.HasValue)
        {
            configurationValues["MessageBroker:MaxRetries"] = maxRetries.Value.ToString();
        }

        if (retryDelayMilliseconds.HasValue)
        {
            configurationValues["MessageBroker:RetryDelayMilliseconds"] = retryDelayMilliseconds.Value.ToString();
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSimpleConsole(o => o.SingleLine = true));
        services.AddSingleton<IConfiguration>(configuration);
        ConsolidationApplicationDependencyInjection.AddApplicationServices(services);
        ConsolidationInfrastructureDependencyInjection.AddInfrastructureServices(services, configuration);

        return services.BuildServiceProvider();
    }

    private static async Task MigrateAsync<TDbContext>(IServiceProvider services) where TDbContext : DbContext
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
