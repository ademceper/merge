using FluentValidation;

namespace Merge.Application.ML.Commands.EvaluatePayment;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class EvaluatePaymentCommandValidator : AbstractValidator<EvaluatePaymentCommand>
{
    public EvaluatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");
    }
}
