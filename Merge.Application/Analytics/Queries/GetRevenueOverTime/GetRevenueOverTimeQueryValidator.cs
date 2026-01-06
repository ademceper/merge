using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetRevenueOverTime;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetRevenueOverTimeQueryValidator : AbstractValidator<GetRevenueOverTimeQuery>
{
    public GetRevenueOverTimeQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur")
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Başlangıç tarihi gelecekte olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Bitiş tarihi gelecekte olamaz");

        RuleFor(x => x.Interval)
            .NotEmpty().WithMessage("Interval zorunludur")
            .Must(x => x == "day" || x == "week" || x == "month")
            .WithMessage("Interval 'day', 'week' veya 'month' olmalıdır");
    }
}

