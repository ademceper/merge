using FluentValidation;

namespace Merge.Application.Content.Commands.GenerateProductSEO;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GenerateProductSEOCommandValidator : AbstractValidator<GenerateProductSEOCommand>
{
    public GenerateProductSEOCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

