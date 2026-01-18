using FluentValidation;

namespace Merge.Application.Seller.Commands.CompletePayout;

public class CompletePayoutCommandValidator : AbstractValidator<CompletePayoutCommand>
{
    public CompletePayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty().WithMessage("Payout ID is required.");
    }
}
