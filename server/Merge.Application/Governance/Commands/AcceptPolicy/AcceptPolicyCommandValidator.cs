using FluentValidation;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

public class AcceptPolicyCommandValidator() : AbstractValidator<AcceptPolicyCommand>
{
    public AcceptPolicyCommandValidator()
    {
    }
}

