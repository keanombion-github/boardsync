using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.CreateBoard;

/// <summary>
/// Handles the business logic for creating a board.
/// It takes the validated command, runs the SQL via Dapper, and returns the new board ID.
/// </summary>
public class CreateBoardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateBoardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Guid> HandleAsync(CreateBoardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // Parameterized query prevents SQL injection
        const string sql = """
            INSERT INTO boards (name, owner_id)
            VALUES (@Name, @OwnerId)
            RETURNING id;
            """;

        // Dapper automatically maps the command properties to the @Name and @OwnerId parameters
        var boardId = await connection.ExecuteScalarAsync<Guid>(sql, command);
        
        return boardId;
    }
}
