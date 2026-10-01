using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.UpdateBoard;

public sealed class UpdateBoardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UpdateBoardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(UpdateBoardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            UPDATE boards
            SET name = @Name,
                updated_at = NOW()
            WHERE id = @Id
              AND owner_id = @UserId;
            """;

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            command.Id,
            command.UserId,
            Name = command.Name.Trim()
        });

        return rowsAffected == 1;
    }
}
