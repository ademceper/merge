using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetActivePolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetActivePolicyQueryValidator : AbstractValidator<GetActivePolicyQuery>
{
    public GetActivePolicyQueryValidator()
    {
        RuleFor(x => x.PolicyType)
            .NotEmpty().WithMessage("Policy type gereklidir")
            .MaximumLength(100).WithMessage("Policy type en fazla 100 karakter olabilir");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language gereklidir")
            .MaximumLength(10).WithMessage("Language en fazla 10 karakter olabilir");
    }
}

