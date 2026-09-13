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

        const string sql = @"
            UPDATE cards
            SET title = @Title, description = @Description, updated_at = NOW()
            WHERE id = @Id
        ";

        var rowsAffected = await connection.ExecuteAsync(sql, new {command.Title, command.Description, command.Id});
        if (rowsAffected == 0)
            return false;  // signals "not found" → endpoint returns 404
        return true;  // or just return, endpoint returns 200

    }
}