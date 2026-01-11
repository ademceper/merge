namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductRecommendationDto(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string ImageUrl,
    decimal Rating,
    int ReviewCount,
    string RecommendationReason,
    decimal RecommendationScore
);
