using FluentValidation;

namespace Merge.Application.Subscription.Commands.CreateUserSubscription;

public class CreateUserSubscriptionCommandValidator : AbstractValidator<CreateUserSubscriptionCommand>
{
    public CreateUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.SubscriptionPlanId)
            .NotEmpty().WithMessage("Abonelik planı ID zorunludur.");

        RuleFor(x => x.PaymentMethodId)
            .MaximumLength(100).WithMessage("Ödeme yöntemi ID en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethodId));
    }
}
