using FluentValidation;

namespace Merge.Application.Content.Commands.DeletePageBuilder;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeletePageBuilderCommandValidator : AbstractValidator<DeletePageBuilderCommand>
{
    public DeletePageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

