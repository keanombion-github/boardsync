using FluentValidation;

namespace Boardsync.Api.Features.Cards.UpdateCard;

public class UpdateCardValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Card title is required.")
            .MaximumLength(200).WithMessage("Card title cannot exceed 200 characters.");
        
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");
    }
}