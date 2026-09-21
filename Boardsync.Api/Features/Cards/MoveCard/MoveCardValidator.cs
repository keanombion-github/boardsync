using FluentValidation;

namespace Boardsync.Api.Features.Cards.MoveCard;

public class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ColumnId).NotEmpty();
    }
}