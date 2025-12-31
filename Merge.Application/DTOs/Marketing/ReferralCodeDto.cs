namespace Merge.Application.DTOs.Marketing;

public class ReferralCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int MaxUsage { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int PointsReward { get; set; }
    public decimal DiscountPercentage { get; set; }
}
