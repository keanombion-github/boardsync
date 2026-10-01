using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Columns.DeleteColumn;

public class DeleteColumnHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DeleteColumnHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(DeleteColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM columns AS c
            USING boards AS b
            WHERE c.id = @ColumnId
              AND c.board_id = @BoardId
              AND b.id = c.board_id
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId));
            """;

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new
            {
                command.ColumnId,
                command.BoardId,
                command.UserId
            });

        return rowsAffected > 0;
    }
}
