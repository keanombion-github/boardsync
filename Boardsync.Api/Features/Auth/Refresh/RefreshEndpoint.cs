using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;

namespace Boardsync.Api.Features.Auth.Refresh;

public static class RefreshEndpoint
{
    public static void MapRefreshEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (
            HttpRequest request,
            HttpResponse response,
            RefreshHandler handler,
            RefreshTokenCookieService cookieService) =>
        {
            if (!request.Cookies.TryGetValue(
                    RefreshTokenCookieService.CookieName,
                    out var rawToken)
                || string.IsNullOrWhiteSpace(rawToken))
            {
                return Unauthorized();
            }

            var result = await handler.HandleAsync(rawToken);
            if (result.Code == RefreshResultCode.InvalidToken)
            {
                cookieService.Delete(response);
                return Unauthorized();
            }

            var session = result.Session
                ?? throw new InvalidOperationException(
                    "Successful refresh did not return a session.");

            cookieService.Write(
                response,
                session.RefreshToken.Token,
                session.RefreshToken.ExpiresAtUtc);

            return Results.Ok(
                ApiResponse<AuthResponse>.Ok(
                    AuthResponse.FromSession(session)));
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth-session")
        .WithName("RefreshSession")
        .WithTags("Auth");
    }

    private static IResult Unauthorized()
    {
        return Results.Json(
            ApiResponse<object>.Fail(
                "INVALID_REFRESH_TOKEN",
                "The session is missing, expired, or invalid."),
            statusCode: StatusCodes.Status401Unauthorized);
    }
}
