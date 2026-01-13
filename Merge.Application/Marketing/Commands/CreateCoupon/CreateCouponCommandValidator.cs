using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateCoupon;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kupon kodu zorunludur.")
            .MaximumLength(50).WithMessage("Kupon kodu en fazla 50 karakter olabilir.")
            .MinimumLength(2).WithMessage("Kupon kodu en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Kupon açıklaması zorunludur.")
            .MaximumLength(500).WithMessage("Kupon açıklaması en fazla 500 karakter olabilir.");

        RuleFor(x => x)
            .Must(x => x.DiscountAmount.HasValue || x.DiscountPercentage.HasValue)
            .WithMessage("Kupon için indirim tutarı veya yüzdesi belirtilmelidir.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue)
            .WithMessage("İndirim tutarı negatif olamaz.");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue)
            .WithMessage("İndirim yüzdesi 0-100 arasında olmalıdır.");

        RuleFor(x => x.MinimumPurchaseAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumPurchaseAmount.HasValue)
            .WithMessage("Minimum alışveriş tutarı negatif olamaz.");

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MaximumDiscountAmount.HasValue)
            .WithMessage("Maksimum indirim tutarı negatif olamaz.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.UsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Kullanım limiti negatif olamaz.");
    }
}
