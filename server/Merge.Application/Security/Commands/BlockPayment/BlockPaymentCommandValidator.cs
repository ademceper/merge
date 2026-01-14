using FluentValidation;

namespace Merge.Application.Security.Commands.BlockPayment;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class BlockPaymentCommandValidator : AbstractValidator<BlockPaymentCommand>
{
    public BlockPaymentCommandValidator()
    {
        RuleFor(x => x.CheckId)
            .NotEmpty().WithMessage("CheckId is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(2000).WithMessage("Reason cannot exceed 2000 characters");
    }
}
