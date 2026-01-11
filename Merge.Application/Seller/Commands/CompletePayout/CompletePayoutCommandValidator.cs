using FluentValidation;

namespace Merge.Application.Seller.Commands.CompletePayout;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CompletePayoutCommandValidator : AbstractValidator<CompletePayoutCommand>
{
    public CompletePayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty().WithMessage("Payout ID is required.");
    }
}
