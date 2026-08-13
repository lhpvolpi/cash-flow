using CashFlow.Transactions.Application;
using CashFlow.Transactions.Infrastructure;
using CashFlow.Transactions.Web;
using CashFlow.Transactions.Web.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();

    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow Transactions API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CashFlow - Transactions API";
    });
}
else
{
    app.UseHsts();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck =>
        healthCheck.Tags.Contains("ready")
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// endpoints
app.MapTransactionEndpoints();

app.UseExceptionHandler();

app.Run();
