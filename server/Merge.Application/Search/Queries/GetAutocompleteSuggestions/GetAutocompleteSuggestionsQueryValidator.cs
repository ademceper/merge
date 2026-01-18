using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

public class GetAutocompleteSuggestionsQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetAutocompleteSuggestionsQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetAutocompleteSuggestionsQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Arama terimi boş olamaz.")
            .MinimumLength(config.MinAutocompleteQueryLength)
            .WithMessage($"Arama terimi en az {config.MinAutocompleteQueryLength} karakter olmalıdır.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxAutocompleteResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxAutocompleteResults} olabilir.");
    }
}
