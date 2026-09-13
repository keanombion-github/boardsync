namespace Boardsync.Api.Features.Columns.DeleteColumn;

public class DeleteColumnCommand
{
    public required Guid ColumnId { get; set; }
}