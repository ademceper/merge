using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

public class DeleteEmailAutomationCommandValidator : AbstractValidator<DeleteEmailAutomationCommand>
{
    public DeleteEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
