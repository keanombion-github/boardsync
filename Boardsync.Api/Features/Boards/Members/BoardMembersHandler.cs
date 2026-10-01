using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Boards.Members;

public sealed class BoardMembersHandler(IDbConnectionFactory dbConnectionFactory)
{
    public async Task<BoardMemberDto[]?> ListAsync(Guid boardId, Guid userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        const string accessSql = """
            SELECT EXISTS (
                SELECT 1 FROM boards AS board
                WHERE board.id = @BoardId
                  AND (board.owner_id = @UserId
                       OR EXISTS (SELECT 1 FROM board_members AS member
                                  WHERE member.board_id = board.id AND member.user_id = @UserId)));
            """;
        if (!await connection.ExecuteScalarAsync<bool>(
                accessSql, new { BoardId = boardId, UserId = userId }))
            return null;

        const string membersSql = """
            SELECT users.id AS Id, users.display_name AS DisplayName,
                   users.email AS Email, TRUE AS IsOwner
            FROM boards AS board
            JOIN users ON users.id = board.owner_id
            WHERE board.id = @BoardId
            UNION ALL
            SELECT users.id AS Id, users.display_name AS DisplayName,
                   users.email AS Email, FALSE AS IsOwner
            FROM board_members AS member
            JOIN users ON users.id = member.user_id
            WHERE member.board_id = @BoardId
            ORDER BY IsOwner DESC, DisplayName, Id;
            """;
        return (await connection.QueryAsync<BoardMemberDto>(
            membersSql, new { BoardId = boardId })).ToArray();
    }

    public async Task<AddMemberResult> AddAsync(
        Guid boardId, Guid ownerId, string email)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string ownerSql = """
            SELECT id FROM boards
            WHERE id = @BoardId AND owner_id = @OwnerId
            FOR UPDATE;
            """;
        var board = await connection.QuerySingleOrDefaultAsync<Guid?>(
            ownerSql, new { BoardId = boardId, OwnerId = ownerId }, transaction);
        if (board is null)
            return AddMemberResult.BoardNotFound;

        const string userSql = "SELECT id FROM users WHERE email = @Email;";
        var memberId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            userSql, new { Email = email }, transaction);
        if (memberId is null)
            return AddMemberResult.UserNotFound;
        if (memberId == ownerId)
            return AddMemberResult.AlreadyMember;

        const string insertSql = """
            INSERT INTO board_members (board_id, user_id)
            VALUES (@BoardId, @MemberId)
            ON CONFLICT DO NOTHING;
            """;
        var inserted = await connection.ExecuteAsync(insertSql,
            new { BoardId = boardId, MemberId = memberId.Value }, transaction);
        transaction.Commit();
        return inserted == 0 ? AddMemberResult.AlreadyMember : AddMemberResult.Added;
    }

    public async Task<bool> RemoveAsync(Guid boardId, Guid ownerId, Guid memberId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        const string ownerSql = """
            SELECT id FROM boards
            WHERE id = @BoardId AND owner_id = @OwnerId
            FOR UPDATE;
            """;
        var board = await connection.QuerySingleOrDefaultAsync<Guid?>(ownerSql,
            new { BoardId = boardId, OwnerId = ownerId }, transaction);
        if (board is null)
            return false;

        const string deleteSql = """
            DELETE FROM board_members
            WHERE board_id = @BoardId AND user_id = @MemberId;
            """;
        var removed = await connection.ExecuteAsync(deleteSql,
            new { BoardId = boardId, MemberId = memberId }, transaction);
        if (removed == 0)
            return false;

        const string clearAssignmentsSql = """
            UPDATE cards AS card
            SET assigned_to = NULL, updated_at = NOW()
            FROM columns AS col
            WHERE card.column_id = col.id
              AND col.board_id = @BoardId
              AND card.assigned_to = @MemberId;
            """;
        await connection.ExecuteAsync(clearAssignmentsSql,
            new { BoardId = boardId, MemberId = memberId }, transaction);

        transaction.Commit();
        return true;
    }
}

public sealed record BoardMemberDto(Guid Id, string DisplayName, string Email, bool IsOwner);
public enum AddMemberResult { Added, BoardNotFound, UserNotFound, AlreadyMember }
