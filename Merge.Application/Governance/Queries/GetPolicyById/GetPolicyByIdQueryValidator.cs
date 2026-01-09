using FluentValidation;

namespace Merge.Application.Governance.Queries.GetPolicyById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetPolicyByIdQueryValidator : AbstractValidator<GetPolicyByIdQuery>
{
    public GetPolicyByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID gereklidir");
    }
}

