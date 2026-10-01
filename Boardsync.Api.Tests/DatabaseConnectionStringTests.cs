using Boardsync.Api.Common.Database;
using Npgsql;

namespace Boardsync.Api.Tests;

public sealed class DatabaseConnectionStringTests
{
    [Fact]
    public void FromPostgresUri_ConvertsRenderStyleUri()
    {
        var result = DatabaseConnectionString.FromPostgresUri(
            "postgresql://board%40user:p%3Ass@db.internal:5433/boardsync_prod");

        var parsed = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal("db.internal", parsed.Host);
        Assert.Equal(5433, parsed.Port);
        Assert.Equal("boardsync_prod", parsed.Database);
        Assert.Equal("board@user", parsed.Username);
        Assert.Equal("p:ss", parsed.Password);
        Assert.True(parsed.Pooling);
    }

    [Fact]
    public void FromPostgresUri_PreservesNpgsqlConnectionString()
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=boardsync;Username=postgres;Password=test";

        Assert.Equal(
            connectionString,
            DatabaseConnectionString.FromPostgresUri(connectionString));
    }
}
