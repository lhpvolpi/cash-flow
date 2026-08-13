using CashFlow.Auth.Application.AuthTokens.Login.Commands;

namespace CashFlow.Auth.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithName("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithDescription("Authenticate with username and password and receive a JWT");
    }

    private static async Task<IResult> Login(
        LoginCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
