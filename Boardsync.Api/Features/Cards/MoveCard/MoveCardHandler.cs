using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Cards.MoveCard;

public class MoveCardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MoveCardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(MoveCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // 1. Calculate new position
        var newPosition = CalculatePosition(command.BeforePosition, command.AfterPosition);
        // 2. Update both column_id and position in one SQL statement
        const string sql = @"
            UPDATE cards 
            SET column_id = @ColumnId, position = @Position, updated_at = NOW()
            WHERE id = @Id
        ";
        var rowsAffected = await connection.ExecuteAsync(sql, 
            new { command.ColumnId, Position = newPosition, command.Id });
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

