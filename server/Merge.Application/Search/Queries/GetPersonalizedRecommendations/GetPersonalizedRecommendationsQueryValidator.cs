using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetPersonalizedRecommendations;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPersonalizedRecommendationsQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetPersonalizedRecommendationsQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetPersonalizedRecommendationsQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si boş olamaz.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxRecommendationResults} olabilir.");
    }
}
