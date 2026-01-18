using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetAnalyticsSummary;

public class GetAnalyticsSummaryQueryValidator : AbstractValidator<GetAnalyticsSummaryQuery>
{
    public GetAnalyticsSummaryQueryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThanOrEqualTo(1).WithMessage("Gün sayısı 1 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(365).WithMessage("Gün sayısı 365'den küçük veya eşit olmalıdır");
    }
}

