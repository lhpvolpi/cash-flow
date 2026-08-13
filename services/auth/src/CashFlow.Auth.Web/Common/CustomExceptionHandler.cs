using CashFlow.Auth.Application.Common.Exceptions;
using CashFlow.Shared.Application.Common;

namespace CashFlow.Auth.Web.Common;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<CustomExceptionHandler> _logger;
    private const string ProblemJsonContentType = "application/problem+json";

    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler(IHostEnvironment environment, ILogger<CustomExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;

        _exceptionHandlers = new()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(InvalidCredentialsException), HandleInvalidCredentialsException }
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (_exceptionHandlers.TryGetValue(exception.GetType(), out var handler))
        {
            await handler.Invoke(httpContext, exception);
            return true;
        }

        await HandleUnknownException(httpContext, exception);
        return true;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = ProblemJsonContentType;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = "Validation failed.",
            Instance = httpContext.Request.Path
        });
    }

    private async Task HandleInvalidCredentialsException(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.ContentType = ProblemJsonContentType;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.2",
            Title = "Invalid credentials.",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        });
    }

    private async Task HandleUnknownException(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = ProblemJsonContentType;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Detail = _environment.IsDevelopment() ? exception.Message : null,
            Instance = httpContext.Request.Path
        });
    }
}
