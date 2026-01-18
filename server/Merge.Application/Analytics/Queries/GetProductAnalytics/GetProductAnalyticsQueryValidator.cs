using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetProductAnalytics;

public class GetProductAnalyticsQueryValidator : AbstractValidator<GetProductAnalyticsQuery>
{
    public GetProductAnalyticsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.StartDate.HasValue)
            .WithMessage("Başlangıç tarihi gelecekte olamaz");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi gelecekte olamaz");
    }
}

