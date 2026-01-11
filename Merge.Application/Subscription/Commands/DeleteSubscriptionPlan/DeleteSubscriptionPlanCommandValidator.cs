using FluentValidation;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class DeleteSubscriptionPlanCommandValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID zorunludur.");
    }
}
