using FluentValidation;

namespace Boardsync.Api.Features.Boards.CreateBoard;

/// <summary>
/// Validates the CreateBoardCommand before it reaches the handler.
/// This prevents bad data from ever hitting the database or core logic.
/// </summary>
public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Board name is required.")
            .MaximumLength(200).WithMessage("Board name cannot exceed 200 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
