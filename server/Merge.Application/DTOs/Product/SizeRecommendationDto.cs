using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record SizeRecommendationDto(
    string RecommendedSize,
    string Confidence, // High, Medium, Low
    IReadOnlyList<string> AlternativeSizes,
    string Reasoning
);
