using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

public class DeactivatePolicyCommandValidator : AbstractValidator<DeactivatePolicyCommand>
{
    public DeactivatePolicyCommandValidator()
    {
    }
}

