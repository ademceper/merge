using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.DeletePolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeletePolicyCommandValidator : AbstractValidator<DeletePolicyCommand>
{
    public DeletePolicyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

