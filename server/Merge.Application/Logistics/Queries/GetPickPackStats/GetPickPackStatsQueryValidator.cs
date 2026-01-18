using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetPickPackStats;

public class GetPickPackStatsQueryValidator : AbstractValidator<GetPickPackStatsQuery>
{
    public GetPickPackStatsQueryValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.");
    }
}

