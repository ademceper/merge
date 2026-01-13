using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class DeleteEmailAutomationCommandValidator() : AbstractValidator<DeleteEmailAutomationCommand>
{
    public DeleteEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Automation ID zorunludur.");
    }
}
