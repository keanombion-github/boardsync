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
                c.position  AS ColumnPosition,
                card.id          AS CardId,
                card.title       AS CardTitle,
                card.description AS CardDescription,
                card.position    AS CardPosition
            FROM boards b
            LEFT JOIN columns c ON c.board_id = b.id
            LEFT JOIN cards card ON card.column_id = c.id
            WHERE b.id = @BoardId
            ORDER BY c.position, card.position;
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
            .GroupBy(r => r.ColumnId!.Value)
            .Select(group =>
            {
                var columnRow = group.First();

                var cards = group
                .Where(row => row.CardId.HasValue)
                .Select(row => new CardDto(
                    row.CardId!.Value,
                    row.CardTitle!,
                    row.CardDescription,
                    row.CardPosition!.Value
                ))
                .ToArray();

                return new ColumnDto(
                    columnRow.ColumnId!.Value,
                    columnRow.ColumnName!,
                    columnRow.ColumnPosition!.Value,
                    cards
                );

            })
            .ToArray();

        return new BoardDetailDto(firstRow.Id, firstRow.Name, columns);
    }

    private record BoardRow(
        Guid Id,
        string Name,
        Guid? ColumnId,
        string? ColumnName,
        double? ColumnPosition,
        Guid? CardId,
        string? CardTitle,
        string? CardDescription,
        double? CardPosition
    );
}

// DTOs — defined here, in the same slice
public record ColumnDto(Guid Id, string Name, double Position, IEnumerable<CardDto> Cards);
public record BoardDetailDto(Guid Id, string Name, IEnumerable<ColumnDto> Columns);

public record CardDto(Guid Id, string Title, string? Description, double Position);