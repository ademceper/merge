using FluentValidation;

namespace Merge.Application.Subscription.Commands.ActivateSubscription;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");
    }
}
