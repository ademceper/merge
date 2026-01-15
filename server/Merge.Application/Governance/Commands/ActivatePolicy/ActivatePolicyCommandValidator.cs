using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

public class ActivatePolicyCommandValidator : AbstractValidator<ActivatePolicyCommand>
{
    public ActivatePolicyCommandValidator()
    {
    }
}

