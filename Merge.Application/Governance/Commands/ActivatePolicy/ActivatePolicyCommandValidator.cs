using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class ActivatePolicyCommandValidator : AbstractValidator<ActivatePolicyCommand>
{
    public ActivatePolicyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

