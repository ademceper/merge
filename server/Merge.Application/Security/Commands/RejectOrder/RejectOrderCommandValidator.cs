using FluentValidation;

namespace Merge.Application.Security.Commands.RejectOrder;

public class RejectOrderCommandValidator : AbstractValidator<RejectOrderCommand>
{
    public RejectOrderCommandValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEmpty().WithMessage("VerificationId is required");

        RuleFor(x => x.VerifiedByUserId)
            .NotEmpty().WithMessage("VerifiedByUserId is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(2000).WithMessage("Reason cannot exceed 2000 characters");
    }
}
