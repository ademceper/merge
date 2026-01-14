using FluentValidation;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeleteOldAuditLogsCommandValidator : AbstractValidator<DeleteOldAuditLogsCommand>
{
    public DeleteOldAuditLogsCommandValidator()
    {
        RuleFor(x => x.DaysToKeep)
            .GreaterThan(0).WithMessage("Days to keep 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(3650).WithMessage("Days to keep en fazla 3650 (10 yıl) olabilir");
    }
}
