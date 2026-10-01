using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Ordering;
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
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string cardScopeSql = """
            SELECT col.board_id AS BoardId
            FROM cards AS c
            JOIN columns AS col ON col.id = c.column_id
            JOIN boards AS b ON b.id = col.board_id
            WHERE c.id = @Id
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId));
            """;

        var sourceBoardId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            cardScopeSql,
            new { command.Id, command.UserId },
            transaction);

        if (!sourceBoardId.HasValue)
            return MoveCardResult.CardNotFound;

        const string boardLockSql = """
            SELECT id
            FROM boards
            WHERE id = @BoardId
            FOR UPDATE;
            """;

        await connection.ExecuteScalarAsync<Guid>(
            boardLockSql,
            new { BoardId = sourceBoardId.Value },
            transaction);

        const string destinationSql = """
            SELECT col.board_id
            FROM columns AS col
            JOIN boards AS b ON b.id = col.board_id
            WHERE col.id = @ColumnId
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId))
            FOR UPDATE OF col;
            """;

        var destinationBoardId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            destinationSql,
            new { command.ColumnId, command.UserId },
            transaction);

        if (!destinationBoardId.HasValue)
            return MoveCardResult.DestinationColumnNotFound;

        if (sourceBoardId.Value != destinationBoardId.Value)
            return MoveCardResult.CrossBoardMove;

        const string destinationCardsSql = """
            SELECT id, position
            FROM cards
            WHERE column_id = @ColumnId AND id <> @Id
            ORDER BY position, id
            FOR UPDATE;
            """;

        var destinationCards = (
            await connection.QueryAsync<PositionedItem>(
                destinationCardsSql,
                new { command.ColumnId, command.Id },
                transaction)
        ).AsList();

        if (!FractionalPosition.TryCalculate(
                destinationCards,
                command.BeforeCardId,
                command.AfterCardId,
                out var newPosition))
        {
            return MoveCardResult.InvalidNeighbors;
        }

        const string updateSql = """
            UPDATE cards
            SET column_id = @ColumnId,
                position = @Position,
                updated_at = NOW()
            WHERE id = @Id;
            """;

        var rowsAffected = await connection.ExecuteAsync(
            updateSql,
            new
            {
                command.Id,
                command.ColumnId,
                Position = newPosition
            },
            transaction);

        if (rowsAffected == 0)
            return MoveCardResult.CardNotFound;

        transaction.Commit();
        return MoveCardResult.Moved;
    }
}
