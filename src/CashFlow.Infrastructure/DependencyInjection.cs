using CashFlow.Application.Abstractions;
using CashFlow.Infrastructure.Data;
using CashFlow.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
    }
}
