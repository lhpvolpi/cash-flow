using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Infrastructure.Data;
using CashFlow.Consolidation.Infrastructure.Data.Repositories;
using CashFlow.Consolidation.Infrastructure.MessageBroker;
using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Application.Models;

namespace CashFlow.Consolidation.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ApplicationDbContextMigrator>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IProcessedTransactionRepository, ProcessedTransactionRepository>();

        // RabbitMQ
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("RabbitMq connection string not found in appsettings.json");

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(rabbitMqConnectionString),
                DispatchConsumersAsync = true
            };
            return factory.CreateConnection();
        });

        services.AddScoped<IMessageBrokerConsumer<BrokerMessage>, RabbitMqMessageBrokerConsumer>();
        services.AddScoped<IConsolidationMessageBrokerConsumer>(sp =>
            sp.GetRequiredService<IMessageBrokerConsumer<BrokerMessage>>() as RabbitMqMessageBrokerConsumer
            ?? throw new InvalidOperationException("Failed to resolve RabbitMqMessageBrokerConsumer"));

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "postgres",
                tags: new List<string> { "ready" })
            .AddRabbitMQ(
                name: "rabbitmq",
                tags: new List<string> { "ready" });
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var applicationDbContextInitialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextMigrator>();
        await applicationDbContextInitialiser.ApplyMigrationsAsync();
    }
}


