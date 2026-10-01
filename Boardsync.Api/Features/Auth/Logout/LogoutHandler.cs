using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Auth.Logout;

public sealed class LogoutHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IAuthTokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public LogoutHandler(
        IDbConnectionFactory dbConnectionFactory,
        IAuthTokenService tokenService,
        TimeProvider timeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(string rawToken)
    {
        const string sql = """
            UPDATE refresh_tokens
            SET revoked_at = @RevokedAtUtc
            WHERE token_hash = @TokenHash
              AND revoked_at IS NULL;
            """;

        using var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            sql,
            new
            {
                TokenHash = _tokenService.HashRefreshToken(rawToken),
                RevokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
            });
    }
}
