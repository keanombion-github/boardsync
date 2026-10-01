namespace Boardsync.Api.Features.Boards.CreateBoard;

/// <summary>
/// The data required to create a new board.
/// In Vertical Slice Architecture, we define commands/queries close to where they are used.
/// </summary>
public class CreateBoardCommand
{
    public required string Name { get; set; }
    public required Guid OwnerId { get; set; }
}

public sealed record CreateBoardBody(string Name);
