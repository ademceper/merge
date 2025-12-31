namespace Merge.Application.DTOs.Marketing;

public class LoyaltyAccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int TierLevel { get; set; }
    public DateTime? TierExpiresAt { get; set; }
}
