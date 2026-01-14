using FluentValidation;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class AcceptPolicyCommandValidator : AbstractValidator<AcceptPolicyCommand>
{
    public AcceptPolicyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID gereklidir");

        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

