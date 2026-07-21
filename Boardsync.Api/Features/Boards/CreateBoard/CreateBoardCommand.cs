namespace Boardsync.Api.Features.Boards.CreateBoard;

/// <summary>
/// The data required to create a new board.
/// In Vertical Slice Architecture, we define commands/queries close to where they are used.
/// </summary>
public class CreateBoardCommand
{
    public required string Name { get; set; }
    
    // Note: Once we add JWT Authentication, we will extract this from the token claims.
    // For now, we will pass it explicitly to test the endpoint.
    public required Guid OwnerId { get; set; }
}
