using FluentValidation;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ToggleEmailAutomationCommandValidator : AbstractValidator<ToggleEmailAutomationCommand>
{
    public ToggleEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
