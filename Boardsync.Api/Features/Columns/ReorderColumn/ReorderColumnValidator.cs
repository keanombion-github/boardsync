using FluentValidation;

namespace Boardsync.Api.Features.Columns.ReorderColumn;

public class ReorderColumnValidator : AbstractValidator<ReorderColumnCommand>
{
    public ReorderColumnValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.ColumnId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.BeforeColumnId != x.ColumnId
                && x.AfterColumnId != x.ColumnId)
            .WithMessage("A column cannot be its own neighbor.");

        RuleFor(x => x)
            .Must(x => !x.BeforeColumnId.HasValue
                || !x.AfterColumnId.HasValue
                || x.BeforeColumnId != x.AfterColumnId)
            .WithMessage("Before and after columns must be different.");
    }
}
