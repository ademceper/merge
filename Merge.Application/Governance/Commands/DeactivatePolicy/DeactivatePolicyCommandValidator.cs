using FluentValidation;

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeactivatePolicyCommandValidator : AbstractValidator<DeactivatePolicyCommand>
{
    public DeactivatePolicyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

