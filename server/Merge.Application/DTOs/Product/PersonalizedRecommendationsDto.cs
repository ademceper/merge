using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record PersonalizedRecommendationsDto(
    IReadOnlyList<ProductRecommendationDto> ForYou,
    IReadOnlyList<ProductRecommendationDto> BasedOnHistory,
    IReadOnlyList<ProductRecommendationDto> Trending,
    IReadOnlyList<ProductRecommendationDto> BestSellers
);
