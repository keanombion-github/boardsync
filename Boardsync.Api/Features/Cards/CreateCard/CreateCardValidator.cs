using FluentValidation;

namespace Boardsync.Api.Features.Cards.CreateCard;

public class CreateCardValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Card title is required.")
            .MaximumLength(200).WithMessage("Card title cannot exceed 200 characters.");
        
        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("Column ID is required.")
            .Custom((value, context) => 
            {
                if (value == Guid.Empty) context.AddFailure("Column ID must be a valid UUID.");
            });
    }
}