using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

public class GetPendingPoliciesQueryValidator() : AbstractValidator<GetPendingPoliciesQuery>
{
    public GetPendingPoliciesQueryValidator()
    {
    }
}

