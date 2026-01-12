using FluentValidation;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.CompareChanges;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class CompareChangesQueryValidator : AbstractValidator<CompareChangesQuery>
{
    public CompareChangesQueryValidator()
    {
        RuleFor(x => x.AuditLogId)
            .NotEmpty().WithMessage("Audit log ID gereklidir");
    }
}
