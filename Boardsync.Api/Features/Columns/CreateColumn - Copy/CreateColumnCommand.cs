namespace Boardsync.Api.Features.Columns.CreateColumn;

public record CreateColumnBody(string Name);

public class CreateColumnCommand 
{
    public required string Name { get; set; }
    public required Guid BoardId { get; set; }
}