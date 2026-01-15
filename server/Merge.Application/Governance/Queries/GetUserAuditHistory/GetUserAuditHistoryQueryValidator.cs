using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetUserAuditHistory;

public class GetUserAuditHistoryQueryValidator() : AbstractValidator<GetUserAuditHistoryQuery>
{
    public GetUserAuditHistoryQueryValidator()
    {
    }
}

