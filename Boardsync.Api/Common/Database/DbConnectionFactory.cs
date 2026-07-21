using System.Data;
using Npgsql;

namespace Boardsync.Api.Common.Database;

/// <summary>
/// Factory for creating database connections.
/// Registered as a singleton in DI — it only holds the connection string,
/// not an open connection. Each call to CreateConnection() returns a new connection.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
