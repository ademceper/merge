using FluentValidation;

namespace Merge.Application.Governance.Queries.GetAuditStats;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetAuditStatsQueryValidator : AbstractValidator<GetAuditStatsQuery>
{
    public GetAuditStatsQueryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThan(0).WithMessage("Days 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(365).WithMessage("Days en fazla 365 olabilir");
    }
}

