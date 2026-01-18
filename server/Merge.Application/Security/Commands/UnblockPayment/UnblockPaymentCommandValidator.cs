using FluentValidation;

namespace Merge.Application.Security.Commands.UnblockPayment;

public class UnblockPaymentCommandValidator : AbstractValidator<UnblockPaymentCommand>
{
    public UnblockPaymentCommandValidator()
    {
        RuleFor(x => x.CheckId)
            .NotEmpty().WithMessage("CheckId is required");
    }
}
