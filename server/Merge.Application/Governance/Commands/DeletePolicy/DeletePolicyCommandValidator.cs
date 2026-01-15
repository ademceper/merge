using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.DeletePolicy;

public class DeletePolicyCommandValidator() : AbstractValidator<DeletePolicyCommand>
{
    public DeletePolicyCommandValidator()
    {
    }
}

