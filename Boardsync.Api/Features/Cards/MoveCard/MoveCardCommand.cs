namespace Boardsync.Api.Features.Cards.MoveCard;

public record MoveCardBody(
    Guid ColumnId,
    Guid? BeforeCardId,
    Guid? AfterCardId);

public enum MoveCardResult
{
    Moved,
    CardNotFound,
    DestinationColumnNotFound,
    CrossBoardMove,
    InvalidNeighbors
}

public class MoveCardCommand
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid ColumnId { get; set; }
    public Guid? BeforeCardId { get; set; }
    public Guid? AfterCardId { get; set; }
}
