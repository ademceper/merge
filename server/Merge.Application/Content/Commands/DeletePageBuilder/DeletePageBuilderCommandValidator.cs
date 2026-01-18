using FluentValidation;

namespace Merge.Application.Content.Commands.DeletePageBuilder;

public class DeletePageBuilderCommandValidator : AbstractValidator<DeletePageBuilderCommand>
{
    public DeletePageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

