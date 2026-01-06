using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetDashboardMetricsQueryValidator : AbstractValidator<GetDashboardMetricsQuery>
{
    public GetDashboardMetricsQueryValidator()
    {
        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

