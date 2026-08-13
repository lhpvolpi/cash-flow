using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CashFlow.Auth.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddProblemDetails();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddHealthChecks();

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "CashFlow Auth API",
                Version = "v1",
                Description = "API for issuing authentication tokens",
                Contact = new()
                {
                    Name = "CashFlow Team",
                    Url = new Uri("https://github.com")
                },
                License = new()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
        });
    }
}
