using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.GetBoardById;

public class GetBoardByIdHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetBoardByIdHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<BoardDetailDto?> HandleAsync(GetBoardByIdQuery query)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                b.id        AS Id,
                b.name      AS Name,
                c.id        AS ColumnId,
                c.name      AS ColumnName,
                c.position  AS ColumnPosition
            FROM boards b
            LEFT JOIN columns c ON c.board_id = b.id
            WHERE b.id = @BoardId
            ORDER BY c.position;
            """;

        // Step 1: fetch all flat rows
        var rows = await connection.QueryAsync<BoardRow>(sql, new { query.BoardId });

        // Step 2: if no rows at all, the board doesn't exist
        if (!rows.Any())
            return null;

        // Step 3: all rows share the same board — take the first row for board data
        var firstRow = rows.First();

        // Step 4: build the columns list, filtering out null entries (empty board)
        var columns = rows
            .Where(r => r.ColumnId.HasValue)
            .Select(r => new ColumnDto(r.ColumnId!.Value, r.ColumnName!, r.ColumnPosition!.Value));

        return new BoardDetailDto(firstRow.Id, firstRow.Name, columns);
    }

    private record BoardRow(
        Guid Id,
        string Name,
        Guid? ColumnId,
        string? ColumnName,
        double? ColumnPosition
    );
}

// DTOs — defined here, in the same slice
public record ColumnDto(Guid Id, string Name, double Position);
public record BoardDetailDto(Guid Id, string Name, IEnumerable<ColumnDto> Columns);