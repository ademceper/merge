using FluentValidation;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

public class DeleteFraudDetectionRuleCommandValidator : AbstractValidator<DeleteFraudDetectionRuleCommand>
{
    public DeleteFraudDetectionRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");
    }
}
