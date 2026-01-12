using FluentValidation;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class RevokeAcceptanceCommandValidator : AbstractValidator<RevokeAcceptanceCommand>
{
    public RevokeAcceptanceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID gereklidir");

        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

