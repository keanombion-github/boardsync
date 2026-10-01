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
                (b.owner_id = @UserId) AS IsOwner,
                c.id        AS ColumnId,
                c.name      AS ColumnName,
                c.position  AS ColumnPosition,
                card.id          AS CardId,
                card.title       AS CardTitle,
                card.description AS CardDescription,
                card.position    AS CardPosition,
                card.assigned_to AS CardAssigneeId,
                assignee.display_name AS CardAssigneeName
            FROM boards b
            LEFT JOIN columns c ON c.board_id = b.id
            LEFT JOIN cards card ON card.column_id = c.id
            LEFT JOIN users assignee ON assignee.id = card.assigned_to
            WHERE b.id = @BoardId
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId))
            ORDER BY c.position, c.id, card.position, card.id;
            """;

        // Step 1: fetch all flat rows
        var rows = await connection.QueryAsync<BoardRow>(
            sql,
            new { query.BoardId, query.UserId });

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
                    row.CardPosition!.Value,
                    row.CardAssigneeId,
                    row.CardAssigneeName
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

        return new BoardDetailDto(firstRow.Id, firstRow.Name, firstRow.IsOwner, columns);
    }

    private record BoardRow(
        Guid Id,
        string Name,
        bool IsOwner,
        Guid? ColumnId,
        string? ColumnName,
        double? ColumnPosition,
        Guid? CardId,
        string? CardTitle,
        string? CardDescription,
        double? CardPosition,
        Guid? CardAssigneeId,
        string? CardAssigneeName
    );
}

// DTOs — defined here, in the same slice
public record ColumnDto(Guid Id, string Name, double Position, IEnumerable<CardDto> Cards);
public record BoardDetailDto(Guid Id, string Name, bool IsOwner, IEnumerable<ColumnDto> Columns);

public record CardDto(
    Guid Id, string Title, string? Description, double Position,
    Guid? AssigneeId, string? AssigneeName);
