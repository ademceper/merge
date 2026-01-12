using FluentValidation;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.CreateProductBundle;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateProductBundleCommandValidator : AbstractValidator<CreateProductBundleCommand>
{
    public CreateProductBundleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Paket adı boş olamaz.")
            .MaximumLength(200).WithMessage("Paket adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.BundlePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Paket fiyatı 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir.");

        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("En az bir ürün seçilmelidir.")
            .Must(p => p.Count > 0).WithMessage("En az bir ürün seçilmelidir.");

        RuleForEach(x => x.Products)
            .SetValidator(new AddProductToBundleDtoValidator());

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
    }
}

public class AddProductToBundleDtoValidator : AbstractValidator<AddProductToBundleDto>
{
    public AddProductToBundleDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
    }
}
