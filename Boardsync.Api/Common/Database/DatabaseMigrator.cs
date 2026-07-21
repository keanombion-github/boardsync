using DbUp;

namespace Boardsync.Api.Common.Database;

/// <summary>
/// Runs SQL migration scripts using DbUp.
/// 
/// How DbUp works:
/// 1. It scans for embedded resource SQL files (our migration scripts)
/// 2. It checks a journal table (schemaversions) to see which scripts already ran
/// 3. It runs only the new scripts, in alphabetical/numerical order
/// 4. It records each successful script in the journal table
/// 
/// This runs at app startup — so every time the API starts, the database
/// is guaranteed to be up to date.
/// </summary>
public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        // EnsureDatabase.For.PostgresqlDatabase creates the database if it doesn't exist.
        // Since we already created it manually, this is a safety net.
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .WithTransaction()       // Each script runs in a transaction — if it fails, it rolls back
            .LogToConsole()           // Logs migration progress to the console
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception($"Database migration failed: {result.Error.Message}", result.Error);
        }
    }
}
