using FluentValidation;

namespace Merge.Application.ML.Commands.CreateFraudDetectionRule;

public class CreateFraudDetectionRuleCommandValidator : AbstractValidator<CreateFraudDetectionRuleCommand>
{
    public CreateFraudDetectionRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.RuleType)
            .NotEmpty().WithMessage("Rule type is required.")
            .MaximumLength(50).WithMessage("Rule type cannot exceed 50 characters.");

        RuleFor(x => x.RiskScore)
            .InclusiveBetween(0, 100).WithMessage("Risk score must be between 0 and 100.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(50).WithMessage("Action cannot exceed 50 characters.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
