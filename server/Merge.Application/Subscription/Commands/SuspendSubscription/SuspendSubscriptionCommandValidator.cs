using FluentValidation;

namespace Merge.Application.Subscription.Commands.SuspendSubscription;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
{
    public SuspendSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");
    }
}
