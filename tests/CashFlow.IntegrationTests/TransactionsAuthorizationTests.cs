extern alias TransactionsWeb;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionsProgram = TransactionsWeb::Program;

namespace CashFlow.IntegrationTests;

public class TransactionsAuthorizationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public TransactionsAuthorizationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateTransaction_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions", new { amount = 10m, type = "Credit" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions", new { amount = 10m, type = "Credit" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_WithValidToken_CreatesTransaction()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestTokenFactory.GenerateValidToken());

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions", new { amount = 10m, type = "Credit" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private WebApplicationFactory<TransactionsProgram> CreateFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _fixture.TransactionsDbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__RabbitMq", _fixture.RabbitMqConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", IntegrationTestFixture.JwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", IntegrationTestFixture.JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", IntegrationTestFixture.JwtAudience);

        return new WebApplicationFactory<TransactionsProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }
}
