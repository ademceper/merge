using FluentValidation;

namespace Merge.Application.Product.Commands.UpdateProductBundle;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateProductBundleCommandValidator : AbstractValidator<UpdateProductBundleCommand>
{
    public UpdateProductBundleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Paket adı boş olamaz.")
            .MaximumLength(200).WithMessage("Paket adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.BundlePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Paket fiyatı 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
    }
}
