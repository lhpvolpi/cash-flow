using CashFlow.Consolidation.Application;
using CashFlow.Consolidation.Infrastructure;
using CashFlow.Consolidation.Consumer;

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
    .ConfigureWebHostDefaults(builder =>
    {
        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        });
    })
    .Build();

await host.RunAsync();

