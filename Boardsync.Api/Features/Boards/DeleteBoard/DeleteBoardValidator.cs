using FluentValidation;

namespace Boardsync.Api.Features.Boards.DeleteBoard;

public sealed class DeleteBoardValidator : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
