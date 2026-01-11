using FluentValidation;

namespace Merge.Application.Order.Queries.GetOrderStatistics;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetOrderStatisticsQueryValidator : AbstractValidator<GetOrderStatisticsQuery>
{
    public GetOrderStatisticsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
    }
}
