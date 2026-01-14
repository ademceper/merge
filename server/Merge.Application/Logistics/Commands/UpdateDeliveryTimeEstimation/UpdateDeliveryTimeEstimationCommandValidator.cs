using FluentValidation;

namespace Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateDeliveryTimeEstimationCommandValidator : AbstractValidator<UpdateDeliveryTimeEstimationCommand>
{
    public UpdateDeliveryTimeEstimationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teslimat süresi tahmini ID'si zorunludur.");

        RuleFor(x => x.MinDays)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum gün 0 veya daha büyük olmalıdır.")
            .When(x => x.MinDays.HasValue);

        RuleFor(x => x.MaxDays)
            .GreaterThanOrEqualTo(0).WithMessage("Maksimum gün 0 veya daha büyük olmalıdır.")
            .When(x => x.MaxDays.HasValue);

        RuleFor(x => x.AverageDays)
            .GreaterThanOrEqualTo(0).WithMessage("Ortalama gün 0 veya daha büyük olmalıdır.")
            .When(x => x.AverageDays.HasValue);

        // Cross-property validation
        When(x => x.MinDays.HasValue && x.MaxDays.HasValue, () =>
        {
            RuleFor(x => x.MaxDays)
                .GreaterThanOrEqualTo(x => x.MinDays)
                .WithMessage("Maksimum gün minimum günden küçük olamaz.");
        });

        When(x => x.AverageDays.HasValue && x.MinDays.HasValue && x.MaxDays.HasValue, () =>
        {
            RuleFor(x => x.AverageDays)
                .GreaterThanOrEqualTo(x => x.MinDays)
                .WithMessage("Ortalama gün minimum günden küçük olamaz.")
                .LessThanOrEqualTo(x => x.MaxDays)
                .WithMessage("Ortalama gün maksimum günden büyük olamaz.");
        });
    }
}

