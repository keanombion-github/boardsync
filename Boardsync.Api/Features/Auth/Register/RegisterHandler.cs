using Boardsync.Api.Common.Database;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Boardsync.Api.Features.Auth.Register;

public sealed class RegisterHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IPasswordHasher<string> _passwordHasher;

    public RegisterHandler(
        IDbConnectionFactory dbConnectionFactory,
        IPasswordHasher<string> passwordHasher)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> HandleAsync(
        RegisterCommand command)
    {
        var normalizedEmail = command.Email
            .Trim()
            .ToLowerInvariant();

        var displayName = command.DisplayName.Trim();

        var passwordHash = _passwordHasher.HashPassword(
            normalizedEmail,
            command.Password);

        const string sql = """
            INSERT INTO users (
                email,
                password_hash,
                display_name
            )
            VALUES (
                @Email,
                @PasswordHash,
                @DisplayName
            )
            ON CONFLICT (email) DO NOTHING
            RETURNING id;
            """;

        using var connection =
            _dbConnectionFactory.CreateConnection();

        var userId =
            await connection.QuerySingleOrDefaultAsync<Guid?>(
                sql,
                new
                {
                    Email = normalizedEmail,
                    PasswordHash = passwordHash,
                    DisplayName = displayName
                });

        if (!userId.HasValue)
            return RegisterResult.EmailConflict();

        return RegisterResult.Success(userId.Value);
    }
}