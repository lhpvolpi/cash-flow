using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashFlow.IntegrationTests;

public sealed class HealthChecksTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public HealthChecksTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TransactionsHealthChecks_WhenDependenciesAreReachable_ReportHealthy()
    {
        // Arrange
        var healthCheckService = _fixture.TransactionsServices.GetRequiredService<HealthCheckService>();

        // Act
        var report = await healthCheckService.CheckHealthAsync();

        // Assert
        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Contains("postgres", report.Entries.Keys);
        Assert.Contains("rabbitmq", report.Entries.Keys);
    }

    [Fact]
    public async Task ConsolidationHealthChecks_WhenDependenciesAreReachable_ReportHealthy()
    {
        // Arrange
        var healthCheckService = _fixture.ConsolidationServices.GetRequiredService<HealthCheckService>();

        // Act
        var report = await healthCheckService.CheckHealthAsync();

        // Assert
        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Contains("postgres", report.Entries.Keys);
        Assert.Contains("rabbitmq", report.Entries.Keys);
    }
}
