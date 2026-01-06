using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetReferralPerformance;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetReferralPerformanceQueryValidator : AbstractValidator<GetReferralPerformanceQuery>
{
    public GetReferralPerformanceQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur")
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Başlangıç tarihi gelecekte olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Bitiş tarihi gelecekte olamaz");
    }
}

