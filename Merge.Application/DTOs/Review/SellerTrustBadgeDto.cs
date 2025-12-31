namespace Merge.Application.DTOs.Review;

public class SellerTrustBadgeDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public Guid TrustBadgeId { get; set; }
    public TrustBadgeDto TrustBadge { get; set; } = null!;
    public DateTime AwardedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string? AwardReason { get; set; }
}
