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
            SELECT id
            FROM columns
            WHERE id = @ColumnId
            FOR UPDATE;
            """;

        var columnId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            columnLockSql,
            new { command.ColumnId },
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
