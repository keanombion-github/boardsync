using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.GetBoards;


public class GetBoardsHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBoardsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<BoardDto>> HandleAsync(GetBoardsQuery query) {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT id, name Name FROM boards WHERE owner_id = @OwnerId;
        """;

        var board = await connection.QueryAsync<BoardDto>(sql, new { OwnerId = query.OwnerId });

        return board;

    }

}

public record BoardDto(Guid Id, string Name);
