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

    public async Task<Guid> HandleAsync(CreateCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

          const string positionQuery = @"
            SELECT COALESCE(MAX(position), 0) 
            FROM cards
            WHERE column_id = @ColumnId
        ";

        var maxPosition = await connection.ExecuteScalarAsync<double>(positionQuery, new { command.ColumnId });
        
        var newPosition = maxPosition + 1.0;

        // Parameterized query prevents SQL injection
        const string sql = @"
            INSERT INTO cards (title, description, column_id, position)
            VALUES (@Title, @Description, @ColumnId, @Position)
            RETURNING id;
            ";

        // Dapper automatically maps the command properties to the @Name and @BoardId parameters
        var cardId = await connection.ExecuteScalarAsync<Guid>(sql, new { command.Title, command.Description, command.ColumnId, Position = newPosition });
        
        return cardId;

    }
}