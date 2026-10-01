namespace Boardsync.Api.Features.Columns.ReorderColumn;

public record ReorderColumnBody(
    Guid ColumnId,
    Guid? BeforeColumnId,
    Guid? AfterColumnId);

public enum ReorderColumnResult
{
    Reordered,
    BoardNotFound,
    ColumnNotFound,
    InvalidNeighbors
}

public class ReorderColumnCommand
{
    public required Guid BoardId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid ColumnId { get; set; }
    public Guid? BeforeColumnId { get; set; }
    public Guid? AfterColumnId { get; set; }
}
