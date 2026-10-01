using Boardsync.Api.Common.Database;
using Dapper;


namespace Boardsync.Api.Features.Cards.UpdateCard;

public class UpdateCardHandler 
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UpdateCardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(UpdateCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            UPDATE cards AS card
            SET title = @Title,
                description = @Description,
                updated_at = NOW()
            FROM columns AS col
            JOIN boards AS b ON b.id = col.board_id
            WHERE card.id = @Id
              AND card.column_id = col.id
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId));
            """;

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new
            {
                command.Title,
                command.Description,
                command.Id,
                command.UserId
            });
        if (rowsAffected == 0)
            return false;  // signals "not found" → endpoint returns 404
        return true;  // or just return, endpoint returns 200

    }
}
