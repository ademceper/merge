namespace Merge.Application.DTOs.Marketing;


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
