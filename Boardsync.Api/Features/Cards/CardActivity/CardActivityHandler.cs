using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Cards.CardActivity;

public sealed class CardActivityHandler(IDbConnectionFactory dbConnectionFactory)
{
    public async Task<CardActivityDto?> GetAsync(Guid cardId, Guid userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string cardSql = """
            SELECT card.id AS Id, card.title AS Title,
                   card.description AS Description, card.assigned_to AS AssigneeId,
                   assignee.display_name AS AssigneeName
            FROM cards AS card
            JOIN columns AS col ON col.id = card.column_id
            JOIN boards AS board ON board.id = col.board_id
            LEFT JOIN users AS assignee ON assignee.id = card.assigned_to
            WHERE card.id = @CardId
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId));
            """;

        var card = await connection.QuerySingleOrDefaultAsync<CardActivityRow>(
            cardSql, new { CardId = cardId, UserId = userId });
        if (card is null)
            return null;

        const string commentsSql = """
            SELECT comment.id AS Id, comment.body AS Body,
                   author.display_name AS AuthorName, comment.author_id AS AuthorId,
                   comment.created_at AS CreatedAt
            FROM card_comments AS comment
            JOIN users AS author ON author.id = comment.author_id
            WHERE comment.card_id = @CardId
            ORDER BY comment.created_at, comment.id;
            """;
        const string reactionsSql = """
            SELECT emoji AS Emoji, COUNT(*)::int AS Count,
                   BOOL_OR(user_id = @UserId) AS ReactedByMe
            FROM card_reactions
            WHERE card_id = @CardId
            GROUP BY emoji
            ORDER BY emoji;
            """;
        const string attachmentsSql = """
            SELECT id AS Id, label AS Label, url AS Url,
                   added_by AS AddedBy, created_at AS CreatedAt
            FROM card_attachments
            WHERE card_id = @CardId
            ORDER BY created_at, id;
            """;

        var comments = await connection.QueryAsync<CardCommentDto>(
            commentsSql, new { CardId = cardId });
        var reactions = await connection.QueryAsync<CardReactionDto>(
            reactionsSql, new { CardId = cardId, UserId = userId });
        var attachments = await connection.QueryAsync<CardAttachmentDto>(
            attachmentsSql, new { CardId = cardId });

        return new CardActivityDto(card.Id, card.Title, card.Description,
            card.AssigneeId, card.AssigneeName, comments.ToArray(), reactions.ToArray(),
            attachments.ToArray());
    }

    public async Task<Guid?> AddCommentAsync(Guid cardId, Guid userId, string body)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO card_comments (card_id, author_id, body)
            SELECT card.id, @UserId, @Body
            FROM cards AS card
            JOIN columns AS col ON col.id = card.column_id
            JOIN boards AS board ON board.id = col.board_id
            WHERE card.id = @CardId
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId))
            RETURNING id;
            """;
        return await connection.QuerySingleOrDefaultAsync<Guid?>(
            sql, new { CardId = cardId, UserId = userId, Body = body });
    }

    public async Task<bool> ToggleReactionAsync(Guid cardId, Guid userId, string emoji)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string cardLockSql = """
            SELECT card.id FROM cards AS card
            JOIN columns AS col ON col.id = card.column_id
            JOIN boards AS board ON board.id = col.board_id
            WHERE card.id = @CardId
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId))
            FOR UPDATE OF card;
            """;
        var found = await connection.QuerySingleOrDefaultAsync<Guid?>(
            cardLockSql, new { CardId = cardId, UserId = userId }, transaction);
        if (found is null)
            return false;

        const string deleteSql = """
            DELETE FROM card_reactions
            WHERE card_id = @CardId AND user_id = @UserId AND emoji = @Emoji;
            """;
        var removed = await connection.ExecuteAsync(deleteSql,
            new { CardId = cardId, UserId = userId, Emoji = emoji }, transaction);
        if (removed == 0)
        {
            const string insertSql = """
                INSERT INTO card_reactions (card_id, user_id, emoji)
                VALUES (@CardId, @UserId, @Emoji);
                """;
            await connection.ExecuteAsync(insertSql,
                new { CardId = cardId, UserId = userId, Emoji = emoji }, transaction);
        }

        transaction.Commit();
        return true;
    }

    public async Task<Guid?> AddAttachmentAsync(
        Guid cardId, Guid userId, string label, string url)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO card_attachments (card_id, added_by, label, url)
            SELECT card.id, @UserId, @Label, @Url
            FROM cards AS card
            JOIN columns AS col ON col.id = card.column_id
            JOIN boards AS board ON board.id = col.board_id
            WHERE card.id = @CardId
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId))
            RETURNING id;
            """;
        return await connection.QuerySingleOrDefaultAsync<Guid?>(sql,
            new { CardId = cardId, UserId = userId, Label = label, Url = url });
    }

    public async Task<bool> DeleteAttachmentAsync(
        Guid cardId, Guid attachmentId, Guid userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        const string sql = """
            DELETE FROM card_attachments AS attachment
            USING cards AS card, columns AS col, boards AS board
            WHERE attachment.id = @AttachmentId
              AND attachment.card_id = @CardId
              AND card.id = attachment.card_id
              AND col.id = card.column_id
              AND board.id = col.board_id
              AND (board.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = board.id AND member.user_id = @UserId));
            """;
        return await connection.ExecuteAsync(sql,
            new { CardId = cardId, AttachmentId = attachmentId, UserId = userId }) > 0;
    }

    private sealed record CardActivityRow(
        Guid Id, string Title, string? Description, Guid? AssigneeId,
        string? AssigneeName);
}

public sealed record CardActivityDto(
    Guid Id, string Title, string? Description, Guid? AssigneeId,
    string? AssigneeName, CardCommentDto[] Comments, CardReactionDto[] Reactions,
    CardAttachmentDto[] Attachments);

public sealed record CardCommentDto(
    Guid Id, string Body, string AuthorName, Guid AuthorId, DateTime CreatedAt);

public sealed record CardReactionDto(string Emoji, int Count, bool ReactedByMe);

public sealed record CardAttachmentDto(
    Guid Id, string Label, string Url, Guid AddedBy, DateTime CreatedAt);
