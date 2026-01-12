using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetPolicies;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetPoliciesQueryValidator : AbstractValidator<GetPoliciesQuery>
{
    public GetPoliciesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var maxPageSize = paginationSettings.Value.MaxPageSize;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Page size en fazla {maxPageSize} olabilir");

        RuleFor(x => x.PolicyType)
            .MaximumLength(100).WithMessage("Policy type en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.PolicyType));

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Language en fazla 10 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Language));
    }
}

