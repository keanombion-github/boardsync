using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;
using FluentValidation;

namespace Boardsync.Api.Features.Auth.Login;

public sealed record LoginBody(string Email, string Password);

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginBody body,
            IValidator<LoginCommand> validator,
            LoginHandler handler,
            RefreshTokenCookieService cookieService,
            HttpResponse response) =>
        {
            var command = new LoginCommand
            {
                Email = body.Email,
                Password = body.Password
            };

            var validation = await validator.ValidateAsync(command);
            if (!validation.IsValid)
            {
                var details = validation.Errors
                    .Select(error => error.ErrorMessage);

                return Results.BadRequest(
                    ApiResponse<object>.Fail(
                        "VALIDATION_ERROR",
                        "Invalid login data.",
                        details));
            }

            var result = await handler.HandleAsync(command);
            if (result.Code == LoginResultCode.InvalidCredentials)
            {
                return Results.Json(
                    ApiResponse<object>.Fail(
                        "INVALID_CREDENTIALS",
                        "Email or password is incorrect."),
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var session = result.Session
                ?? throw new InvalidOperationException(
                    "Authenticated login did not return a session.");

            cookieService.Write(
                response,
                session.RefreshToken.Token,
                session.RefreshToken.ExpiresAtUtc);

            return Results.Ok(
                ApiResponse<AuthResponse>.Ok(
                    AuthResponse.FromSession(session)));
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth-sensitive")
        .WithName("Login")
        .WithTags("Auth");
    }
}
