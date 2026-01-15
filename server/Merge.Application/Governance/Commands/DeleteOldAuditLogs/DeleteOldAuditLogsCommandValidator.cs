using FluentValidation;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

public class DeleteOldAuditLogsCommandValidator : AbstractValidator<DeleteOldAuditLogsCommand>
{
    public DeleteOldAuditLogsCommandValidator()
    {
    }
}
