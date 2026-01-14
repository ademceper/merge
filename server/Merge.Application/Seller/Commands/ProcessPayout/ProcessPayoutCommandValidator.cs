using FluentValidation;

namespace Merge.Application.Seller.Commands.ProcessPayout;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ProcessPayoutCommandValidator : AbstractValidator<ProcessPayoutCommand>
{
    public ProcessPayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty().WithMessage("Payout ID is required.");

        RuleFor(x => x.TransactionReference)
            .NotEmpty().WithMessage("Transaction reference is required.")
            .MaximumLength(100).WithMessage("Transaction reference must not exceed 100 characters.");
    }
}
