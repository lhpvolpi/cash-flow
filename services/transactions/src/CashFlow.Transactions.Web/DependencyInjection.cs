using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CashFlow.Transactions.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
        services.AddRouting(options => options.LowercaseUrls = true);

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
                Title = "CashFlow Transactions API",
                Version = "v1",
                Description = "API for managing financial transactions",
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

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
    }
}
