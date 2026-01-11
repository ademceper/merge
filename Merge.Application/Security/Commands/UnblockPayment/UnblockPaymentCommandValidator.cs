using FluentValidation;

namespace Merge.Application.Security.Commands.UnblockPayment;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class UnblockPaymentCommandValidator : AbstractValidator<UnblockPaymentCommand>
{
    public UnblockPaymentCommandValidator()
    {
        RuleFor(x => x.CheckId)
            .NotEmpty().WithMessage("CheckId is required");
    }
}
