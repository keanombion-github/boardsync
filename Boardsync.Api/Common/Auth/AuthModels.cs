namespace Boardsync.Api.Common.Auth;

public sealed record AuthUser(
    Guid Id,
    string Email,
    string DisplayName);

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);

public sealed record AuthSession(
    AuthUser User,
    AccessTokenResult AccessToken,
    RefreshTokenResult RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    AuthUser User)
{
    public static AuthResponse FromSession(AuthSession session)
    {
        return new AuthResponse(
            session.AccessToken.Token,
            session.AccessToken.ExpiresAtUtc,
            session.User);
    }
}
