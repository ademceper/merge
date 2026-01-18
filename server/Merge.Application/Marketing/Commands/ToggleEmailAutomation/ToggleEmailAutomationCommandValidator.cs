using FluentValidation;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

public class ToggleEmailAutomationCommandValidator : AbstractValidator<ToggleEmailAutomationCommand>
{
    public ToggleEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
