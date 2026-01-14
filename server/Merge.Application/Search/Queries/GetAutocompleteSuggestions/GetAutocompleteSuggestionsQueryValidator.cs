using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAutocompleteSuggestionsQueryValidator : AbstractValidator<GetAutocompleteSuggestionsQuery>
{
    public GetAutocompleteSuggestionsQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Arama terimi boş olamaz.")
            .MinimumLength(settings.MinAutocompleteQueryLength)
            .WithMessage($"Arama terimi en az {settings.MinAutocompleteQueryLength} karakter olmalıdır.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxAutocompleteResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {settings.MaxAutocompleteResults} olabilir.");
    }
}
