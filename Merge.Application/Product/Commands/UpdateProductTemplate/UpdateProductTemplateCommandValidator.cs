using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Product.Commands.UpdateProductTemplate;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateProductTemplateCommandValidator : AbstractValidator<UpdateProductTemplateCommand>
{
    public UpdateProductTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Şablon ID boş olamaz.");

        RuleFor(x => x.Name)
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Şablon adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Şablon adı en fazla 200 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).When(x => x.Description != null)
            .WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).When(x => x.Brand != null)
            .WithMessage("Marka en fazla 100 karakter olabilir.");

        RuleFor(x => x.DefaultSKUPrefix)
            .MaximumLength(50).When(x => x.DefaultSKUPrefix != null)
            .WithMessage("SKU öneki en fazla 50 karakter olabilir.");

        RuleFor(x => x.DefaultPrice)
            .GreaterThanOrEqualTo(0).When(x => x.DefaultPrice.HasValue)
            .WithMessage("Varsayılan fiyat 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DefaultStockQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.DefaultStockQuantity.HasValue)
            .WithMessage("Varsayılan stok miktarı 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DefaultImageUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.DefaultImageUrl))
            .WithMessage("Resim URL'si en fazla 500 karakter olabilir.")
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.DefaultImageUrl))
            .WithMessage("Geçerli bir URL giriniz.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
