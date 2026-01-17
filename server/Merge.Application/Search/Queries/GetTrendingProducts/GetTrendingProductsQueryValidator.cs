using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetTrendingProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetTrendingProductsQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetTrendingProductsQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetTrendingProductsQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxTrendingDays)
            .WithMessage($"Gün sayısı en fazla {config.MaxTrendingDays} olabilir.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxRecommendationResults} olabilir.");
    }
}
