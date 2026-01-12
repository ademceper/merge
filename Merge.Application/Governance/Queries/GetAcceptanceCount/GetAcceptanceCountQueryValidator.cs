using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetAcceptanceCount;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetAcceptanceCountQueryValidator : AbstractValidator<GetAcceptanceCountQuery>
{
    public GetAcceptanceCountQueryValidator()
    {
        RuleFor(x => x.PolicyId)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

