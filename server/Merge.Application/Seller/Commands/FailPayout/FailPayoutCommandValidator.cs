using FluentValidation;

namespace Merge.Application.Seller.Commands.FailPayout;

public class FailPayoutCommandValidator : AbstractValidator<FailPayoutCommand>
{
    public FailPayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty().WithMessage("Payout ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
