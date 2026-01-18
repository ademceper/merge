using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

public class GetDashboardMetricsQueryValidator : AbstractValidator<GetDashboardMetricsQuery>
{
    public GetDashboardMetricsQueryValidator()
    {
        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

