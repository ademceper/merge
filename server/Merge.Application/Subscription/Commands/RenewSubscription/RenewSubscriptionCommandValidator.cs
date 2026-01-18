using FluentValidation;

namespace Merge.Application.Subscription.Commands.RenewSubscription;

public class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");
    }
}
