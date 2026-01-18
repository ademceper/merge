using FluentValidation;

namespace Merge.Application.Subscription.Commands.CancelUserSubscription;

public class CancelUserSubscriptionCommandValidator : AbstractValidator<CancelUserSubscriptionCommand>
{
    public CancelUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("İptal nedeni en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
