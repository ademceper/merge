using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Product;

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
