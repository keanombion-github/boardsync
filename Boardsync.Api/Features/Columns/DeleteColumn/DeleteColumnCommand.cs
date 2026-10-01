namespace Boardsync.Api.Features.Columns.DeleteColumn;

public class DeleteColumnCommand
{
    public required Guid BoardId { get; set; }
    public required Guid ColumnId { get; set; }
    public required Guid UserId { get; set; }
}
