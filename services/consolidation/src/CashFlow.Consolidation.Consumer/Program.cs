using CashFlow.Consolidation.Application;
using CashFlow.Consolidation.Consumer;
using CashFlow.Consolidation.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((builder, services) =>
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(builder.Configuration);
        services.AddHealthChecks();
        services.AddHostedService<ProcessBrokerMessagesWorker>();
    })
    .Build();

await host.RunAsync();

