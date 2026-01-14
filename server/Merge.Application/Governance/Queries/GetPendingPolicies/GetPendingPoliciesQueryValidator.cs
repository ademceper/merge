using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetPendingPoliciesQueryValidator : AbstractValidator<GetPendingPoliciesQuery>
{
    public GetPendingPoliciesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID gereklidir");
    }
}

