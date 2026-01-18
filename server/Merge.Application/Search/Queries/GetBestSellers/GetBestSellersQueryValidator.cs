using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetBestSellers;

public class GetBestSellersQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetBestSellersQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetBestSellersQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxRecommendationResults} olabilir.");
    }
}
