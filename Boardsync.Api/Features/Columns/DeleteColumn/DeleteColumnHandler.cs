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
            DELETE FROM columns
            WHERE id = @ColumnId AND board_id = @BoardId;
            """;

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new { command.ColumnId, command.BoardId });

        return rowsAffected > 0;
    }
}
