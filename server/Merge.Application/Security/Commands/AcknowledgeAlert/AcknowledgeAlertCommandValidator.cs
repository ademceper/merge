using FluentValidation;

namespace Merge.Application.Security.Commands.AcknowledgeAlert;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class AcknowledgeAlertCommandValidator : AbstractValidator<AcknowledgeAlertCommand>
{
    public AcknowledgeAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("AlertId is required");

        RuleFor(x => x.AcknowledgedByUserId)
            .NotEmpty().WithMessage("AcknowledgedByUserId is required");
    }
}
