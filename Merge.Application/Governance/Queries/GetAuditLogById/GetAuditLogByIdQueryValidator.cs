using FluentValidation;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetAuditLogByIdQueryValidator : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Audit log ID gereklidir");
    }
}

