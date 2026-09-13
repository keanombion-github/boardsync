namespace Boardsync.Api.Features.Columns.ReorderColumn;

public record ReorderColumnBody(double? BeforePosition, double? AfterPosition, Guid ColumnId);

public class ReorderColumnCommand
{
    public required Guid BoardId { get; set; } // card being move
    public required Guid ColumnId { get; set; } // column id target
    public double? BeforePosition { get; set; } 
    public double? AfterPosition { get; set; } 
}
