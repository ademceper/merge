using FluentValidation;

namespace Merge.Application.Content.Commands.GenerateCategorySEO;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GenerateCategorySEOCommandValidator : AbstractValidator<GenerateCategorySEOCommand>
{
    public GenerateCategorySEOCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

