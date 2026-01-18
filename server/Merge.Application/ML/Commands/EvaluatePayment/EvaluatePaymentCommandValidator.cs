using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.ML.Commands.EvaluatePayment;

public class EvaluatePaymentCommandValidator : AbstractValidator<EvaluatePaymentCommand>
{
    public EvaluatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");
    }
}
