using FluentValidation;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class ToggleEmailAutomationCommandValidator() : AbstractValidator<ToggleEmailAutomationCommand>
{
    public ToggleEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
