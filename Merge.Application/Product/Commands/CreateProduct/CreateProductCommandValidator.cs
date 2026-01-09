using FluentValidation;

namespace Merge.Application.Product.Commands.CreateProduct;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Ürün adı en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Ürün adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Ürün açıklaması zorunludur.")
            .MaximumLength(5000)
            .WithMessage("Ürün açıklaması en fazla 5000 karakter olabilir.");

        RuleFor(x => x.SKU)
            .NotEmpty()
            .WithMessage("SKU zorunludur.")
            .MaximumLength(100)
            .WithMessage("SKU en fazla 100 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Ürün fiyatı 0'dan büyük olmalıdır.");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0)
            .When(x => x.DiscountPrice.HasValue)
            .WithMessage("İndirimli fiyat 0'dan büyük olmalıdır.")
            .LessThan(x => x.Price)
            .When(x => x.DiscountPrice.HasValue)
            .WithMessage("İndirimli fiyat normal fiyattan düşük olmalıdır.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.Brand)
            .NotEmpty()
            .WithMessage("Marka zorunludur.")
            .MaximumLength(100)
            .WithMessage("Marka en fazla 100 karakter olabilir.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("Ürün resmi zorunludur.")
            .MaximumLength(500)
            .WithMessage("Resim URL'si en fazla 500 karakter olabilir.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

