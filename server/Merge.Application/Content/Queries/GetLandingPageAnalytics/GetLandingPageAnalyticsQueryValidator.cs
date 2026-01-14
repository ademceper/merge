using FluentValidation;

namespace Merge.Application.Content.Queries.GetLandingPageAnalytics;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetLandingPageAnalyticsQueryValidator : AbstractValidator<GetLandingPageAnalyticsQuery>
{
    public GetLandingPageAnalyticsQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Landing page ID'si boş olamaz.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
    }
}

