using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.CreateSecurityAlert;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class CreateSecurityAlertCommandValidator : AbstractValidator<CreateSecurityAlertCommand>
{
    public CreateSecurityAlertCommandValidator()
    {
        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("AlertType is required")
            .MaximumLength(100).WithMessage("AlertType cannot exceed 100 characters");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .MaximumLength(50).WithMessage("Severity cannot exceed 50 characters")
            .Must(s => Enum.TryParse<AlertSeverity>(s, true, out _))
            .WithMessage("Invalid Severity. Must be a valid AlertSeverity enum value.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }
}
