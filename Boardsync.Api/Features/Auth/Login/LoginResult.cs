using Boardsync.Api.Common.Auth;

namespace Boardsync.Api.Features.Auth.Login;

public enum LoginResultCode
{
    Authenticated,
    InvalidCredentials
}

public sealed record LoginResult(
    LoginResultCode Code,
    AuthSession? Session)
{
    public static LoginResult Success(AuthSession session) =>
        new(LoginResultCode.Authenticated, session);

    public static LoginResult InvalidCredentials() =>
        new(LoginResultCode.InvalidCredentials, null);
}
