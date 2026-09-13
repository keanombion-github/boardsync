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

        const string sql = @"
            DELETE FROM cards WHERE id = @Id
        ";

        var rowsAffected = await connection.ExecuteAsync(sql, new {command.Id});

        if(rowsAffected == 0)
        {
            return false;
        }

        return true;
    }
}