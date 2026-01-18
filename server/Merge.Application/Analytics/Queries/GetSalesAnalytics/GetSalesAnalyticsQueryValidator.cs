using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetSalesAnalytics;

public class GetSalesAnalyticsQueryValidator : AbstractValidator<GetSalesAnalyticsQuery>
{
    public GetSalesAnalyticsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Başlangıç tarihi gelecekte olamaz");
    }
}

