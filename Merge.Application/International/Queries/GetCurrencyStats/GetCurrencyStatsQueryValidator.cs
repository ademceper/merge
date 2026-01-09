using FluentValidation;

namespace Merge.Application.International.Queries.GetCurrencyStats;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetCurrencyStatsQueryValidator : AbstractValidator<GetCurrencyStatsQuery>
{
    public GetCurrencyStatsQueryValidator()
    {
        // GetCurrencyStatsQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

