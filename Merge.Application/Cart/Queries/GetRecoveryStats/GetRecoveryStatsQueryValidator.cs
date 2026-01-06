using FluentValidation;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetRecoveryStatsQueryValidator : AbstractValidator<GetRecoveryStatsQuery>
{
    public GetRecoveryStatsQueryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThan(0).WithMessage("Gün sayısı 0'dan büyük olmalıdır");
    }
}

