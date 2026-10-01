using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Cards.AssignCard;

public sealed class AssignCardHandler(IDbConnectionFactory dbConnectionFactory)
{
    public async Task<AssignCardResult> HandleAsync(
        Guid cardId, Guid userId, Guid? assigneeId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string cardScopeSql = """
            SELECT board.id AS BoardId, board.owner_id AS OwnerId
            FROM cards AS card
            JOIN columns AS col ON col.id = card.column_id
            JOIN boards AS board ON board.id = col.board_id
            WHERE card.id = @CardId
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId))
            FOR UPDATE OF board;
            """;
        var scope = await connection.QuerySingleOrDefaultAsync<CardScope>(
            cardScopeSql, new { CardId = cardId, UserId = userId }, transaction);
        if (scope is null)
            return AssignCardResult.CardNotFound;

        if (assigneeId is not null && assigneeId != scope.OwnerId)
        {
            const string memberSql = """
                SELECT EXISTS (SELECT 1 FROM board_members
                               WHERE board_id = @BoardId AND user_id = @AssigneeId);
                """;
            if (!await connection.ExecuteScalarAsync<bool>(memberSql,
                    new { scope.BoardId, AssigneeId = assigneeId.Value }, transaction))
                return AssignCardResult.InvalidAssignee;
        }

        const string updateSql = """
            UPDATE cards SET assigned_to = @AssigneeId, updated_at = NOW()
            WHERE id = @CardId;
            """;
        await connection.ExecuteAsync(updateSql,
            new { CardId = cardId, AssigneeId = assigneeId }, transaction);
        transaction.Commit();
        return AssignCardResult.Assigned;
    }

    private sealed record CardScope(Guid BoardId, Guid OwnerId);
}

public enum AssignCardResult { Assigned, CardNotFound, InvalidAssignee }
