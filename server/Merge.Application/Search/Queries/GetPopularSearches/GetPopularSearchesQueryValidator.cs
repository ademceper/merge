using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetPopularSearches;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPopularSearchesQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetPopularSearchesQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetPopularSearchesQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxAutocompleteResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxAutocompleteResults} olabilir.");
    }
}
