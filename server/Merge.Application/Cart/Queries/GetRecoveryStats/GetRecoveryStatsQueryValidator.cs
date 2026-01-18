using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

public class GetRecoveryStatsQueryValidator : AbstractValidator<GetRecoveryStatsQuery>
{
    public GetRecoveryStatsQueryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThan(0).WithMessage("Gün sayısı 0'dan büyük olmalıdır");
    }
}

