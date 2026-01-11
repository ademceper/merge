using FluentValidation;

namespace Merge.Application.Product.Commands.CreateProductTemplate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateProductTemplateCommandValidator : AbstractValidator<CreateProductTemplateCommand>
{
    public CreateProductTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şablon adı boş olamaz.")
            .MinimumLength(2).WithMessage("Şablon adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Şablon adı en fazla 200 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir.");

        RuleFor(x => x.DefaultSKUPrefix)
            .MaximumLength(50).WithMessage("SKU öneki en fazla 50 karakter olabilir.");

        RuleFor(x => x.DefaultPrice)
            .GreaterThanOrEqualTo(0).When(x => x.DefaultPrice.HasValue)
            .WithMessage("Varsayılan fiyat 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DefaultStockQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.DefaultStockQuantity.HasValue)
            .WithMessage("Varsayılan stok miktarı 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DefaultImageUrl)
            .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.DefaultImageUrl))
            .WithMessage("Geçerli bir URL giriniz.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
