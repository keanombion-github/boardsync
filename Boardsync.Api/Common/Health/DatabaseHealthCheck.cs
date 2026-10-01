using Boardsync.Api.Common.Database;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Boardsync.Api.Common.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var result = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT 1;",
                    cancellationToken: cancellationToken));

            return result == 1
                ? HealthCheckResult.Healthy("PostgreSQL is reachable.")
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL returned an unexpected health result.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL is unavailable.",
                exception);
        }
    }
}
