using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AssignSizeGuideToProduct;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AssignSizeGuideToProductCommandValidator : AbstractValidator<AssignSizeGuideToProductCommand>
{
    public AssignSizeGuideToProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.SizeGuideId)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");

        RuleFor(x => x.CustomNotes)
            .MaximumLength(500).WithMessage("Özel notlar en fazla 500 karakter olabilir.");

        RuleFor(x => x.FitDescription)
            .MaximumLength(1000).WithMessage("Uyum açıklaması en fazla 1000 karakter olabilir.");
    }
}
