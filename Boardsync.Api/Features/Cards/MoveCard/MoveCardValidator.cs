using FluentValidation;

namespace Boardsync.Api.Features.Cards.MoveCard;

public class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(x => x)
            .Must(x => x.BeforePosition.HasValue || x.AfterPosition.HasValue)
            .WithMessage("At least one of BeforePosition or AfterPosition must be provided.");
    }
}