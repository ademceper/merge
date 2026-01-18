using FluentValidation;

namespace Merge.Application.Content.Commands.UnpublishPageBuilder;

public class UnpublishPageBuilderCommandValidator : AbstractValidator<UnpublishPageBuilderCommand>
{
    public UnpublishPageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

