namespace Boardsync.Api.Features.Boards.DeleteBoard;

public sealed record DeleteBoardCommand(Guid Id, Guid UserId);
