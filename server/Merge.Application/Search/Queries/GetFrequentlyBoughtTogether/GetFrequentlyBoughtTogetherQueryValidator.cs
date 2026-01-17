using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetFrequentlyBoughtTogetherQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetFrequentlyBoughtTogetherQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetFrequentlyBoughtTogetherQueryValidator() : this(Options.Create(new SearchSettings()))
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
