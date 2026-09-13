using FluentValidation;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public class ReorderColumnValidator : AbstractValidator<ReorderColumnCommand>
{
    public ReorderColumnValidator()
    {
        RuleFor(x => x)
            .Must(x => x.BeforePosition.HasValue || x.AfterPosition.HasValue)
            .WithMessage("At least one of BeforePosition or AfterPosition must be provided.");
    }
}