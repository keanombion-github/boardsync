using Boardsync.Api.Common.Auth;

namespace Boardsync.Api.Features.Auth.Refresh;

public enum RefreshResultCode
{
    Refreshed,
    InvalidToken
}

public sealed record RefreshResult(
    RefreshResultCode Code,
    AuthSession? Session)
{
    public static RefreshResult Success(AuthSession session) =>
        new(RefreshResultCode.Refreshed, session);

    public static RefreshResult InvalidToken() =>
        new(RefreshResultCode.InvalidToken, null);
}
