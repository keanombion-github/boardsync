using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Columns.CreateColumn;

public class CreateColumnHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateColumnHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Guid> HandleAsync(CreateColumnCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string positionQuery = @"
            SELECT COALESCE(MAX(position), 0) 
            FROM columns
            WHERE board_id = @BoardId
        ";

        var maxPosition = await connection.ExecuteScalarAsync<double>(positionQuery, new { command.BoardId });
        
        var newPosition = maxPosition + 1.0;

        // Parameterized query prevents SQL injection
        const string sql = @"
            INSERT INTO columns (name, board_id, position)
            VALUES (@Name, @BoardId, @Position)
            RETURNING id;
            ";

        // Dapper automatically maps the command properties to the @Name and @BoardId parameters
        var columnId = await connection.ExecuteScalarAsync<Guid>(sql, new { command.Name, command.BoardId, Position = newPosition });
        
        return columnId;
    }
}
