using FluentValidation;

namespace Merge.Application.Subscription.Commands.TrackUsage;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class TrackUsageCommandValidator : AbstractValidator<TrackUsageCommand>
{
    public TrackUsageCommandValidator()
    {
        RuleFor(x => x.UserSubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");

        RuleFor(x => x.Feature)
            .NotEmpty().WithMessage("Özellik adı zorunludur.")
            .MaximumLength(100).WithMessage("Özellik adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Kullanım sayısı 0'dan büyük olmalıdır.");
    }
}
