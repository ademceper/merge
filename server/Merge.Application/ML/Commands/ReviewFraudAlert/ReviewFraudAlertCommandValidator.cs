using FluentValidation;

namespace Merge.Application.ML.Commands.ReviewFraudAlert;

public class ReviewFraudAlertCommandValidator : AbstractValidator<ReviewFraudAlertCommand>
{
    public ReviewFraudAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("Alert ID is required.");

        RuleFor(x => x.ReviewedByUserId)
            .NotEmpty().WithMessage("Reviewed by user ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
