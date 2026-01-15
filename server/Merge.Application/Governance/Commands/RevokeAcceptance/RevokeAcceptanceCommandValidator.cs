using FluentValidation;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

public class RevokeAcceptanceCommandValidator : AbstractValidator<RevokeAcceptanceCommand>
{
    public RevokeAcceptanceCommandValidator()
    {
    }
}

