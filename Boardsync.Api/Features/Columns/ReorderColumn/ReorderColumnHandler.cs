using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Ordering;
using Dapper;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public class ReorderColumnHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ReorderColumnHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<ReorderColumnResult> HandleAsync(
        ReorderColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string boardSql = """
            SELECT id
            FROM boards
            WHERE id = @BoardId
            FOR UPDATE;
            """;

        var boardId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            boardSql,
            new { command.BoardId },
            transaction);

        if (!boardId.HasValue)
            return ReorderColumnResult.BoardNotFound;

        const string columnsSql = """
            SELECT id, position
            FROM columns
            WHERE board_id = @BoardId AND id <> @ColumnId
            ORDER BY position, id
            FOR UPDATE;
            """;

        var columns = (
            await connection.QueryAsync<PositionedItem>(
                columnsSql,
                new { command.BoardId, command.ColumnId },
                transaction)
        ).AsList();

        if (!FractionalPosition.TryCalculate(
                columns,
                command.BeforeColumnId,
                command.AfterColumnId,
                out var newPosition))
        {
            return ReorderColumnResult.InvalidNeighbors;
        }

        const string updateSql = """
            UPDATE columns
            SET position = @Position
            WHERE id = @ColumnId AND board_id = @BoardId;
            """;

        var rowsAffected = await connection.ExecuteAsync(
            updateSql,
            new
            {
                command.ColumnId,
                command.BoardId,
                Position = newPosition
            },
            transaction);

        if (rowsAffected == 0)
            return ReorderColumnResult.ColumnNotFound;

        transaction.Commit();
        return ReorderColumnResult.Reordered;
    }
}
