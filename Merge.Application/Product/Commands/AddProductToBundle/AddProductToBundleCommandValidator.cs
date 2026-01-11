using FluentValidation;

namespace Merge.Application.Product.Commands.AddProductToBundle;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class AddProductToBundleCommandValidator : AbstractValidator<AddProductToBundleCommand>
{
    public AddProductToBundleCommandValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıralama 0 veya daha büyük olmalıdır.");
    }
}
