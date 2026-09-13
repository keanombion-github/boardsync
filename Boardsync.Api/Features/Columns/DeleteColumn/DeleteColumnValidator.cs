using FluentValidation;

namespace Boardsync.Api.Features.Columns.DeleteColumn;

public class DeleteColumnValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnValidator()
    {
        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("Column ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Column ID must be a valid UUID.");
    }
}
