using FluentValidation;

namespace Merge.Application.Content.Queries.GetBlogAnalytics;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetBlogAnalyticsQueryValidator : AbstractValidator<GetBlogAnalyticsQuery>
{
    public GetBlogAnalyticsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
    }
}

