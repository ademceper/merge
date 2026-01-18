using FluentValidation;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

public class DeleteSubscriptionPlanCommandValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID zorunludur.");
    }
}
