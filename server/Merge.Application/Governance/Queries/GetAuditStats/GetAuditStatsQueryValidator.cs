using FluentValidation;

namespace Merge.Application.Governance.Queries.GetAuditStats;

public class GetAuditStatsQueryValidator : AbstractValidator<GetAuditStatsQuery>
{
    public GetAuditStatsQueryValidator()
    {
    }
}

