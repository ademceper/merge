using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Subscription.Commands.CreateSubscriptionPlan;

public class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan adı zorunludur.")
            .MinimumLength(2).WithMessage("Plan adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Plan adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Plan açıklaması en fazla 1000 karakter olabilir.");

        RuleFor(x => x.PlanType)
            .IsInEnum().WithMessage("Geçersiz plan tipi.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Süre en az 1 gün olmalıdır.");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Geçersiz fatura döngüsü.");

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0).WithMessage("Maksimum kullanıcı sayısı en az 1 olmalıdır.");

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0).WithMessage("Deneme süresi negatif olamaz.")
            .When(x => x.TrialDays.HasValue);

        RuleFor(x => x.SetupFee)
            .GreaterThanOrEqualTo(0).WithMessage("Kurulum ücreti negatif olamaz.")
            .When(x => x.SetupFee.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Para birimi zorunludur.")
            .Length(3).WithMessage("Para birimi 3 karakter olmalıdır (ISO 4217).");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası negatif olamaz.");
    }
}
