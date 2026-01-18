using FluentValidation;

namespace Merge.Application.Security.Commands.VerifyOrder;

public class VerifyOrderCommandValidator : AbstractValidator<VerifyOrderCommand>
{
    public VerifyOrderCommandValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEmpty().WithMessage("VerificationId is required");

        RuleFor(x => x.VerifiedByUserId)
            .NotEmpty().WithMessage("VerifiedByUserId is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
