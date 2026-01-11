using FluentValidation;

namespace Merge.Application.Subscription.Commands.RenewSubscription;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");
    }
}
