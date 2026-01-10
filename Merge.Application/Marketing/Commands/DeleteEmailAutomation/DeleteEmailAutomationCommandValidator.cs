using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteEmailAutomationCommandValidator : AbstractValidator<DeleteEmailAutomationCommand>
{
    public DeleteEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
