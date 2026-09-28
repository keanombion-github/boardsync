using FluentValidation;

namespace Boardsync.Api.Features.Cards.MoveCard;

public class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ColumnId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.BeforeCardId != x.Id && x.AfterCardId != x.Id)
            .WithMessage("A card cannot be its own neighbor.");

        RuleFor(x => x)
            .Must(x => !x.BeforeCardId.HasValue
                || !x.AfterCardId.HasValue
                || x.BeforeCardId != x.AfterCardId)
            .WithMessage("Before and after cards must be different.");
    }
}
