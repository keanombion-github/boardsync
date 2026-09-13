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

    public async Task HandleAsync(DeleteColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM columns WHERE id = @ColumnId",
            new { 
                command.ColumnId 
            });
    }
}