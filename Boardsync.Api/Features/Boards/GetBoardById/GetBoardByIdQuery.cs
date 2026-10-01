namespace Boardsync.Api.Features.Boards.GetBoardById;

public class GetBoardByIdQuery 
{
    public required Guid BoardId { get; set; }
    public required Guid UserId { get; set; }
}
