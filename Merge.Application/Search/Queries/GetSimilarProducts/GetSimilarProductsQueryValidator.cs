using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSimilarProductsQueryValidator : AbstractValidator<GetSimilarProductsQuery>
{
    public GetSimilarProductsQueryValidator(IOptions<SearchSettings> searchSettings)
    {
        var settings = searchSettings.Value;

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si boş olamaz.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {settings.MaxRecommendationResults} olabilir.");
    }
}
