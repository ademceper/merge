using FluentValidation;

namespace Merge.Application.International.Queries.GetTranslationStats;

public class GetTranslationStatsQueryValidator : AbstractValidator<GetTranslationStatsQuery>
{
    public GetTranslationStatsQueryValidator()
    {
        // GetTranslationStatsQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

