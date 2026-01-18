using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

public class GetCouponPerformanceQueryValidator : AbstractValidator<GetCouponPerformanceQuery>
{
    public GetCouponPerformanceQueryValidator()
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

