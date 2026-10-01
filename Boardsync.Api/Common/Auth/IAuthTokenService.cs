namespace Boardsync.Api.Common.Auth;

public interface IAuthTokenService
{
    AccessTokenResult CreateAccessToken(AuthUser user);
    RefreshTokenResult CreateRefreshToken();
    string HashRefreshToken(string token);
}
