using FluentValidation;

namespace Merge.Application.Content.Commands.UnpublishPageBuilder;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UnpublishPageBuilderCommandValidator : AbstractValidator<UnpublishPageBuilderCommand>
{
    public UnpublishPageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

