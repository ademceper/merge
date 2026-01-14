namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Loyalty Tier DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record LoyaltyTierDto(
    Guid Id,
    string Name,
    string Description,
    int MinimumPoints,
    decimal DiscountPercentage,
    decimal PointsMultiplier,
    string Benefits,
    string Color,
    int Level);
