using FluentValidation;

namespace Merge.Application.Logistics.Commands.CreateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateDeliveryTimeEstimationCommandValidator : AbstractValidator<CreateDeliveryTimeEstimationCommand>
{
    public CreateDeliveryTimeEstimationCommandValidator()
    {
        RuleFor(x => x.MinDays)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum gün 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.MaxDays)
            .GreaterThanOrEqualTo(0).WithMessage("Maksimum gün 0 veya daha büyük olmalıdır.")
            .GreaterThanOrEqualTo(x => x.MinDays).WithMessage("Maksimum gün minimum günden küçük olamaz.");

        RuleFor(x => x.AverageDays)
            .GreaterThanOrEqualTo(0).WithMessage("Ortalama gün 0 veya daha büyük olmalıdır.")
            .GreaterThanOrEqualTo(x => x.MinDays).WithMessage("Ortalama gün minimum günden küçük olamaz.")
            .LessThanOrEqualTo(x => x.MaxDays).WithMessage("Ortalama gün maksimum günden büyük olamaz.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}

