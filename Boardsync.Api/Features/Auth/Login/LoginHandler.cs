using Boardsync.Api.Common.Auth;
using Boardsync.Api.Common.Database;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Boardsync.Api.Features.Auth.Login;

public sealed class LoginHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IAuthTokenService _tokenService;

    public LoginHandler(
        IDbConnectionFactory dbConnectionFactory,
        IPasswordHasher<string> passwordHasher,
        IAuthTokenService tokenService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        const string userSql = """
            SELECT
                id,
                email,
                password_hash AS PasswordHash,
                display_name AS DisplayName
            FROM users
            WHERE email = @Email;
            """;

        using var connection = _dbConnectionFactory.CreateConnection();

        var user = await connection.QuerySingleOrDefaultAsync<LoginUserRow>(
            userSql,
            new { Email = normalizedEmail });

        if (user is null)
        {
            _passwordHasher.HashPassword(
                normalizedEmail,
                command.Password);

            return LoginResult.InvalidCredentials();
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            normalizedEmail,
            user.PasswordHash,
            command.Password);

        if (verification == PasswordVerificationResult.Failed)
            return LoginResult.InvalidCredentials();

        var refreshToken = _tokenService.CreateRefreshToken();

        connection.Open();
        using var transaction = connection.BeginTransaction();

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            const string updatePasswordSql = """
                UPDATE users
                SET password_hash = @PasswordHash,
                    updated_at = NOW()
                WHERE id = @Id;
                """;

            await connection.ExecuteAsync(
                updatePasswordSql,
                new
                {
                    user.Id,
                    PasswordHash = _passwordHasher.HashPassword(
                        normalizedEmail,
                        command.Password)
                },
                transaction);
        }

        const string refreshTokenSql = """
            INSERT INTO refresh_tokens (
                user_id,
                token_hash,
                expires_at
            )
            VALUES (
                @UserId,
                @TokenHash,
                @ExpiresAtUtc
            );
            """;

        await connection.ExecuteAsync(
            refreshTokenSql,
            new
            {
                UserId = user.Id,
                refreshToken.TokenHash,
                refreshToken.ExpiresAtUtc
            },
            transaction);

        transaction.Commit();

        var authUser = new AuthUser(
            user.Id,
            user.Email,
            user.DisplayName);

        return LoginResult.Success(new AuthSession(
            authUser,
            _tokenService.CreateAccessToken(authUser),
            refreshToken));
    }

    private sealed class LoginUserRow
    {
        public Guid Id { get; init; }
        public required string Email { get; init; }
        public required string PasswordHash { get; init; }
        public required string DisplayName { get; init; }
    }
}
