namespace Boardsync.Api.Features.Cards.UpdateCard;

public record UpdateCardBody(string Title, string? Description);

public record UpdateCardCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description
);
