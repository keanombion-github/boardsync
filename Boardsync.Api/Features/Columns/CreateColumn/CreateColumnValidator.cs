using FluentValidation;

namespace Boardsync.Api.Features.Columns.CreateColumn;

public class CreateColumnValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Column name is required.")
            .MaximumLength(200).WithMessage("Column name cannot exceed 100 characters.");
        
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage("Board ID is required.")
            .Custom((value, context) => 
            {
                if (value == Guid.Empty) context.AddFailure("Board ID must be a valid UUID.");
            });
    }
}