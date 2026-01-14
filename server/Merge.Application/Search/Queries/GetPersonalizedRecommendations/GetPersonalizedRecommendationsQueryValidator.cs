using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetPersonalizedRecommendations;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPersonalizedRecommendationsQueryValidator : AbstractValidator<GetPersonalizedRecommendationsQuery>
{
    public GetPersonalizedRecommendationsQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si boş olamaz.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {settings.MaxRecommendationResults} olabilir.");
    }
}
