using FluentValidation;

namespace Merge.Application.International.Queries.GetCurrencyStats;

public class GetCurrencyStatsQueryValidator : AbstractValidator<GetCurrencyStatsQuery>
{
    public GetCurrencyStatsQueryValidator()
    {
        // GetCurrencyStatsQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

