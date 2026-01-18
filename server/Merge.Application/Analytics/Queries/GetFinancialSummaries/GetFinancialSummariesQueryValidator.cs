using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetFinancialSummaries;

public class GetFinancialSummariesQueryValidator : AbstractValidator<GetFinancialSummariesQuery>
{
    public GetFinancialSummariesQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur")
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Başlangıç tarihi gelecekte olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Bitiş tarihi gelecekte olamaz");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("Period zorunludur")
            .Must(x => x == "daily" || x == "weekly" || x == "monthly")
            .WithMessage("Period 'daily', 'weekly' veya 'monthly' olmalıdır");
    }
}

