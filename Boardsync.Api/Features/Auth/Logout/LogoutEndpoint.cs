using Boardsync.Api.Common.Auth;

namespace Boardsync.Api.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/logout", async (
            HttpRequest request,
            HttpResponse response,
            LogoutHandler handler,
            RefreshTokenCookieService cookieService) =>
        {
            if (request.Cookies.TryGetValue(
                    RefreshTokenCookieService.CookieName,
                    out var rawToken)
                && !string.IsNullOrWhiteSpace(rawToken))
            {
                await handler.HandleAsync(rawToken);
            }

            cookieService.Delete(response);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithName("Logout")
        .WithTags("Auth");
    }
}
