namespace Boardsync.Api.Features.Boards.GetBoards;

/// <summary>
/// Query to retrieve all boards for a specific owner.
/// </summary>
public class GetBoardsQuery
{
    public required Guid OwnerId { get; set; }
}