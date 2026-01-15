using FluentValidation;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.CreateAuditLog;

public class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
    }
}
