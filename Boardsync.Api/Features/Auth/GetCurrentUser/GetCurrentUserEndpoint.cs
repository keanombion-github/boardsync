using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Boardsync.Api.Features.Auth.GetCurrentUser;

public static class GetCurrentUserEndpoint
{
    public static void MapGetCurrentUserEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
        {
            var user = new AuthUser(
                principal.GetRequiredUserId(),
                principal.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? throw new InvalidOperationException(
                        "Authenticated user is missing an email claim."),
                principal.Identity?.Name
                    ?? throw new InvalidOperationException(
                        "Authenticated user is missing a name claim."));

            return Results.Ok(ApiResponse<AuthUser>.Ok(user));
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithTags("Auth");
    }
}
