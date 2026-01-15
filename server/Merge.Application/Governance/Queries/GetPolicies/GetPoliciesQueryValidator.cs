using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetPolicies;

public class GetPoliciesQueryValidator : AbstractValidator<GetPoliciesQuery>
{
    private readonly PaginationSettings settings;

    public GetPoliciesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        settings = paginationSettings.Value;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize).WithMessage($"Page size en fazla {settings.MaxPageSize} olabilir");

        RuleFor(x => x.PolicyType)
            .MaximumLength(100).WithMessage("Policy type en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.PolicyType));

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Language en fazla 10 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Language));
    }
}

