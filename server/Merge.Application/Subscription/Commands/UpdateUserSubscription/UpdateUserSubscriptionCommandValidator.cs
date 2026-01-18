using FluentValidation;

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

public class UpdateUserSubscriptionCommandValidator : AbstractValidator<UpdateUserSubscriptionCommand>
{
    public UpdateUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Abonelik ID zorunludur.");

        RuleFor(x => x.PaymentMethodId)
            .MaximumLength(100).WithMessage("Ödeme yöntemi ID en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethodId));
    }
}
