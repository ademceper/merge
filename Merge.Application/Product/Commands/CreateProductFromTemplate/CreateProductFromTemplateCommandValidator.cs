using FluentValidation;

namespace Merge.Application.Product.Commands.CreateProductFromTemplate;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateProductFromTemplateCommandValidator : AbstractValidator<CreateProductFromTemplateCommand>
{
    public CreateProductFromTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Şablon ID boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması boş olamaz.")
            .MaximumLength(5000).WithMessage("Ürün açıklaması en fazla 5000 karakter olabilir.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU boş olamaz.")
            .MaximumLength(100).WithMessage("SKU en fazla 100 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0).When(x => x.DiscountPrice.HasValue)
            .WithMessage("İndirimli fiyat 0'dan büyük olmalıdır.")
            .LessThan(x => x.Price).When(x => x.DiscountPrice.HasValue)
            .WithMessage("İndirimli fiyat normal fiyattan düşük olmalıdır.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Resim URL'si en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Geçerli bir URL giriniz.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
