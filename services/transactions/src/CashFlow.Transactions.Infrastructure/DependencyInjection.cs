using CashFlow.Infrastructure.Data;
using CashFlow.Shared.Application.Abstractions;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Infrastructure.Data;
using CashFlow.Transactions.Infrastructure.Data.Repositories;
using CashFlow.Transactions.Infrastructure.MessageBroker;

namespace CashFlow.Transactions.Infrastructure;

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
        services.AddScoped<ITransactionRepository, TransactionsRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        // RabbitMQ
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("RabbitMq connection string not found in appsettings.json");

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(rabbitMqConnectionString)
            };
            return factory.CreateConnection();
        });

        services.AddScoped<IOutboxBrokerPublisher, RabbitMqOutboxBrokerPublisher>();

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
