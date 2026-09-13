namespace Boardsync.Api.Features.Cards.MoveCard;

public record MoveCardBody(double? BeforePosition, double? AfterPosition, Guid ColumnId);

public class MoveCardCommand
{
    public required Guid Id { get; set; } // card being move
    public required Guid ColumnId { get; set; } // column id target
    public double? BeforePosition { get; set; } 
    public double? AfterPosition { get; set; } 
}

