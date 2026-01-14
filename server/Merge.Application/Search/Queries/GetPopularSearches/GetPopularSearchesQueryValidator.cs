using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetPopularSearches;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPopularSearchesQueryValidator : AbstractValidator<GetPopularSearchesQuery>
{
    public GetPopularSearchesQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxAutocompleteResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {settings.MaxAutocompleteResults} olabilir.");
    }
}
