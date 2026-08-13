using CashFlow.Auth.Application;
using CashFlow.Auth.Infrastructure;
using CashFlow.Auth.Web;
using CashFlow.Auth.Web.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow Auth API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CashFlow - Auth API";
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

app.UseHttpsRedirection();
app.UseStaticFiles();

// endpoints
app.MapAuthEndpoints();

app.UseExceptionHandler();

app.Run();
