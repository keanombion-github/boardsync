using FluentValidation;

namespace Boardsync.Api.Features.Boards.UpdateBoard;

public sealed class UpdateBoardValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Board name is required.")
            .MaximumLength(200).WithMessage("Board name cannot exceed 200 characters.");
    }
}
