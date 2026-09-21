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

    public async Task<MoveCardResult> HandleAsync(MoveCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string existingCard = @"
            SELECT EXISTS (
                SELECT 1
                FROM cards
                WHERE column_id = @ColumnId
                AND id <> @Id
            );
        ";

        if (!command.BeforePosition.HasValue && !command.AfterPosition.HasValue)
        {
            var hasOtherCards = await connection.ExecuteScalarAsync<bool>(
                existingCard,
                new { command.ColumnId, command.Id }
            );

            if (hasOtherCards)
                return MoveCardResult.NeighborsRequired;
        }

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
       
       if (rowsAffected == 0)
          return MoveCardResult.CardNotFound;

        return MoveCardResult.Moved;

    }

    private static double CalculatePosition(double? before, double? after)
    {
        if (!before.HasValue && !after.HasValue)
            return 1.0;
        if (before.HasValue && after.HasValue)
            return (before.Value + after.Value) / 2;   // midpoint between neighbors
        if (before.HasValue)
            return before.Value + 1.0;                  // dropping at the bottom
        return after!.Value / 2;                        // dropping at the top
    }
}

