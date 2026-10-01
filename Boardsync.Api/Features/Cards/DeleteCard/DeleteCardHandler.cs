using Boardsync.Api.Common.Database;
using Dapper;

namespace Boardsync.Api.Features.Cards.DeleteCard;

public class DeleteCardHandler
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DeleteCardHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HandleAsync(DeleteCardCommand command)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM cards AS card
            USING columns AS col, boards AS b
            WHERE card.id = @Id
              AND card.column_id = col.id
              AND col.board_id = b.id
              AND (b.owner_id = @UserId
                   OR EXISTS (SELECT 1 FROM board_members AS member
                              WHERE member.board_id = b.id AND member.user_id = @UserId));
            """;

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new { command.Id, command.UserId });

        if(rowsAffected == 0)
        {
            return false;
        }

        return true;
    }
}
