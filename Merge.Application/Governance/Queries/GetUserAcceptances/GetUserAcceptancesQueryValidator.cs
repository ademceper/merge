using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetUserAcceptancesQueryValidator : AbstractValidator<GetUserAcceptancesQuery>
{
    public GetUserAcceptancesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID gereklidir");
    }
}

