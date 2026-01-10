using FluentValidation;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteFraudDetectionRuleCommandValidator : AbstractValidator<DeleteFraudDetectionRuleCommand>
{
    public DeleteFraudDetectionRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");
    }
}
