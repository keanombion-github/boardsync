using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public class ReorderColumnHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ReorderColumnHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(ReorderColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // db query logic

        // 1. Calculate new position
        var newPosition = CalculatePosition(command.BeforePosition, command.AfterPosition);
        // 2. Update both column_id and position in one SQL statement
        const string sql = @"
            UPDATE columns
            SET position = @Position
            WHERE id = @ColumnId AND board_id = @BoardId
        ";
        var rowsAffected = await connection.ExecuteAsync(sql, 
            new { command.ColumnId, Position = newPosition, command.BoardId });
        return rowsAffected > 0;
    }

    private static double CalculatePosition(double? before, double? after)
    {
        if (before.HasValue && after.HasValue)
            return (before.Value + after.Value) / 2;   // midpoint between neighbors
        if (before.HasValue)
            return before.Value + 1.0;                  // dropping at the bottom
        return after!.Value / 2;                        // dropping at the top
    }

}