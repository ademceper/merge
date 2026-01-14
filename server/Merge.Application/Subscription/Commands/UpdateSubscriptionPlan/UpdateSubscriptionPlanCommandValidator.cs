using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Subscription.Commands.UpdateSubscriptionPlan;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID zorunludur.");

        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Plan adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Plan adı en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Plan açıklaması en fazla 1000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Süre en az 1 gün olmalıdır.")
            .When(x => x.DurationDays.HasValue);

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0).WithMessage("Deneme süresi negatif olamaz.")
            .When(x => x.TrialDays.HasValue);

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Geçersiz fatura döngüsü.")
            .When(x => x.BillingCycle.HasValue);

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0).WithMessage("Maksimum kullanıcı sayısı en az 1 olmalıdır.")
            .When(x => x.MaxUsers.HasValue);

        RuleFor(x => x.SetupFee)
            .GreaterThanOrEqualTo(0).WithMessage("Kurulum ücreti negatif olamaz.")
            .When(x => x.SetupFee.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Para birimi 3 karakter olmalıdır (ISO 4217).")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası negatif olamaz.")
            .When(x => x.DisplayOrder.HasValue);
    }
}
