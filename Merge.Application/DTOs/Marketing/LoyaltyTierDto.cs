namespace Merge.Application.DTOs.Marketing;

public class LoyaltyTierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MinimumPoints { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal PointsMultiplier { get; set; }
    public string Benefits { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Level { get; set; }
}
