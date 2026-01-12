using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record PersonalizedRecommendationsDto(
    IReadOnlyList<ProductRecommendationDto> ForYou,
    IReadOnlyList<ProductRecommendationDto> BasedOnHistory,
    IReadOnlyList<ProductRecommendationDto> Trending,
    IReadOnlyList<ProductRecommendationDto> BestSellers
);
