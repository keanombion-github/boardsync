using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Columns.CreateColumn;

public class CreateColumnHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateColumnHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Guid?> HandleAsync(CreateColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string boardLockSql = """
            SELECT id
            FROM boards
            WHERE id = @BoardId
            FOR UPDATE;
            """;

        var boardId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            boardLockSql,
            new { command.BoardId },
            transaction);

        if (!boardId.HasValue)
            return null;

        const string positionSql = """
            SELECT COALESCE(MAX(position), 0)
            FROM columns
            WHERE board_id = @BoardId;
            """;

        var maxPosition = await connection.ExecuteScalarAsync<double>(
            positionSql,
            new { command.BoardId },
            transaction);

        const string insertSql = """
            INSERT INTO columns (name, board_id, position)
            VALUES (@Name, @BoardId, @Position)
            RETURNING id;
            """;

        var columnId = await connection.ExecuteScalarAsync<Guid>(
            insertSql,
            new
            {
                command.Name,
                command.BoardId,
                Position = maxPosition + 1.0
            },
            transaction);

        transaction.Commit();
        return columnId;
    }
}
