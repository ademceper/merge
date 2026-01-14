using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveSizeGuideFromProduct;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RemoveSizeGuideFromProductCommandValidator : AbstractValidator<RemoveSizeGuideFromProductCommand>
{
    public RemoveSizeGuideFromProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");
    }
}
