using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Boardsync.Api.Common.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException(
                "The authenticated user does not contain a valid subject claim.");
    }
}
