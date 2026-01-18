using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

public class GetSimilarProductsQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetSimilarProductsQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetSimilarProductsQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si boş olamaz.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxRecommendationResults} olabilir.");
    }
}
