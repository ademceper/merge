using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record SizeRecommendationDto(
    string RecommendedSize,
    string Confidence, // High, Medium, Low
    IReadOnlyList<string> AlternativeSizes,
    string Reasoning
);
