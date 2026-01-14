using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ShowcaseProductCommandValidator : AbstractValidator<ShowcaseProductCommand>
{
    public ShowcaseProductCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");
    }
}

