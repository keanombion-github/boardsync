using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Auth.Refresh;

public sealed class RefreshHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IAuthTokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public RefreshHandler(
        IDbConnectionFactory dbConnectionFactory,
        IAuthTokenService tokenService,
        TimeProvider timeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<RefreshResult> HandleAsync(string rawToken)
    {
        var tokenHash = _tokenService.HashRefreshToken(rawToken);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string findTokenSql = """
            SELECT
                rt.id AS TokenId,
                rt.user_id AS UserId,
                rt.expires_at AS ExpiresAtUtc,
                rt.revoked_at AS RevokedAtUtc,
                rt.replaced_by_token_id AS ReplacedByTokenId,
                u.email,
                u.display_name AS DisplayName
            FROM refresh_tokens AS rt
            JOIN users AS u ON u.id = rt.user_id
            WHERE rt.token_hash = @TokenHash
            FOR UPDATE OF rt;
            """;

        var storedToken = await connection
            .QuerySingleOrDefaultAsync<RefreshTokenRow>(
                findTokenSql,
                new { TokenHash = tokenHash },
                transaction);

        if (storedToken is null)
            return RefreshResult.InvalidToken();

        if (storedToken.RevokedAtUtc.HasValue)
        {
            if (storedToken.ReplacedByTokenId.HasValue)
            {
                const string revokeFamilySql = """
                    UPDATE refresh_tokens
                    SET revoked_at = COALESCE(revoked_at, @Now)
                    WHERE user_id = @UserId
                      AND revoked_at IS NULL;
                    """;

                await connection.ExecuteAsync(
                    revokeFamilySql,
                    new { Now = now, storedToken.UserId },
                    transaction);

                transaction.Commit();
            }

            return RefreshResult.InvalidToken();
        }

        if (storedToken.ExpiresAtUtc <= now)
        {
            const string revokeExpiredSql = """
                UPDATE refresh_tokens
                SET revoked_at = @Now
                WHERE id = @TokenId;
                """;

            await connection.ExecuteAsync(
                revokeExpiredSql,
                new { Now = now, storedToken.TokenId },
                transaction);

            transaction.Commit();
            return RefreshResult.InvalidToken();
        }

        var nextRefreshToken = _tokenService.CreateRefreshToken();

        const string insertSql = """
            INSERT INTO refresh_tokens (
                user_id,
                token_hash,
                expires_at
            )
            VALUES (
                @UserId,
                @TokenHash,
                @ExpiresAtUtc
            )
            RETURNING id;
            """;

        var nextTokenId = await connection.ExecuteScalarAsync<Guid>(
            insertSql,
            new
            {
                storedToken.UserId,
                nextRefreshToken.TokenHash,
                nextRefreshToken.ExpiresAtUtc
            },
            transaction);

        const string revokeCurrentSql = """
            UPDATE refresh_tokens
            SET revoked_at = @Now,
                replaced_by_token_id = @NextTokenId
            WHERE id = @TokenId;
            """;

        await connection.ExecuteAsync(
            revokeCurrentSql,
            new
            {
                Now = now,
                NextTokenId = nextTokenId,
                storedToken.TokenId
            },
            transaction);

        transaction.Commit();

        var user = new AuthUser(
            storedToken.UserId,
            storedToken.Email,
            storedToken.DisplayName);

        return RefreshResult.Success(new AuthSession(
            user,
            _tokenService.CreateAccessToken(user),
            nextRefreshToken));
    }

    private sealed class RefreshTokenRow
    {
        public Guid TokenId { get; init; }
        public Guid UserId { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public DateTime? RevokedAtUtc { get; init; }
        public Guid? ReplacedByTokenId { get; init; }
        public required string Email { get; init; }
        public required string DisplayName { get; init; }
    }
}
