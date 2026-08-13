extern alias ConsolidationWeb;

using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ConsolidationProgram = ConsolidationWeb::Program;

namespace CashFlow.IntegrationTests;

public class ConsolidationAuthorizationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public ConsolidationAuthorizationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetDailyBalance_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/daily-balances/2026-01-01");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDailyBalance_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

        // Act
        var response = await client.GetAsync("/api/daily-balances/2026-01-01");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDailyBalance_WithValidToken_IsAuthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestTokenFactory.GenerateValidToken());

        // Act
        var response = await client.GetAsync("/api/daily-balances/2026-01-01");

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private WebApplicationFactory<ConsolidationProgram> CreateFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _fixture.ConsolidationDbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__RabbitMq", _fixture.RabbitMqConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", IntegrationTestFixture.JwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", IntegrationTestFixture.JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", IntegrationTestFixture.JwtAudience);

        return new WebApplicationFactory<ConsolidationProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }
}
