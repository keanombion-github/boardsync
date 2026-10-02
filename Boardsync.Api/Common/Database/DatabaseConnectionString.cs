using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace Boardsync.Api.Common.Database;

public static class DatabaseConnectionString
{
    public static string Resolve(IConfiguration configuration)
    {
        var configuredValue =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:DefaultConnection or DATABASE_URL.");
        }

        return FromPostgresUri(configuredValue);
    }

    public static string FromPostgresUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return value;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException("DATABASE_URL is not a valid PostgreSQL URI.");
        }

        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("DATABASE_URL must include a database name.");
        }

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = database,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = Uri.UnescapeDataString(userInfo[1]),
            Pooling = true
        };

        var parameters = QueryHelpers.ParseQuery(uri.Query);
        if (parameters.TryGetValue("sslmode", out var sslMode))
        {
            if (!Enum.TryParse<SslMode>(sslMode.ToString(), true, out var parsedSslMode)
                || !Enum.IsDefined(parsedSslMode))
                throw new InvalidOperationException("DATABASE_URL has an invalid sslmode.");

            connectionString.SslMode = parsedSslMode;
        }

        if (parameters.TryGetValue("channel_binding", out var channelBinding))
        {
            if (!Enum.TryParse<ChannelBinding>(channelBinding.ToString(), true, out var parsedChannelBinding)
                || !Enum.IsDefined(parsedChannelBinding))
                throw new InvalidOperationException("DATABASE_URL has an invalid channel_binding.");

            connectionString.ChannelBinding = parsedChannelBinding;
        }

        return connectionString.ConnectionString;
    }
}
