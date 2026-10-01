namespace Boardsync.Api.Features.Boards.UpdateBoard;

public sealed record UpdateBoardCommand(Guid Id, Guid UserId, string Name);
