using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.DeleteBoard;

public sealed class DeleteBoardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DeleteBoardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(DeleteBoardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM boards
            WHERE id = @Id
              AND owner_id = @UserId;
            """;

        return await connection.ExecuteAsync(sql, command) == 1;
    }
}
