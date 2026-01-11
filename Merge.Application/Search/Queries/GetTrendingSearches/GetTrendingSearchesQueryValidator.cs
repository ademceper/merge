using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetTrendingSearches;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetTrendingSearchesQueryValidator : AbstractValidator<GetTrendingSearchesQuery>
{
    public GetTrendingSearchesQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxTrendingDays)
            .WithMessage($"Gün sayısı en fazla {settings.MaxTrendingDays} olabilir.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxAutocompleteResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {settings.MaxAutocompleteResults} olabilir.");
    }
}
