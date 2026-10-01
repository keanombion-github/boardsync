using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Cards.CreateCard;

public class CreateCardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateCardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Guid?> HandleAsync(CreateCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string columnLockSql = """
            SELECT c.id
            FROM columns AS c
            JOIN boards AS b ON b.id = c.board_id
            WHERE c.id = @ColumnId
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId))
            FOR UPDATE OF c;
            """;

        var columnId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            columnLockSql,
            new { command.ColumnId, command.UserId },
            transaction);

        if (!columnId.HasValue)
            return null;

        const string positionSql = """
            SELECT COALESCE(MAX(position), 0)
            FROM cards
            WHERE column_id = @ColumnId;
            """;

        var maxPosition = await connection.ExecuteScalarAsync<double>(
            positionSql,
            new { command.ColumnId },
            transaction);

        const string insertSql = """
            INSERT INTO cards (title, description, column_id, position)
            VALUES (@Title, @Description, @ColumnId, @Position)
            RETURNING id;
            """;

        var cardId = await connection.ExecuteScalarAsync<Guid>(
            insertSql,
            new
            {
                command.Title,
                command.Description,
                command.ColumnId,
                Position = maxPosition + 1.0
            },
            transaction);

        transaction.Commit();
        return cardId;
    }
}
