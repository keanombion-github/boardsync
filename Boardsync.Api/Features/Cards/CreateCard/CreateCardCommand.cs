namespace Boardsync.Api.Features.Cards.CreateCard;

public record CreateCardBody(string Title, string? Description, Guid ColumnId);

public class CreateCardCommand 
{
    public required Guid ColumnId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
}