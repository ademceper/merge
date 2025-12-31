namespace Merge.Application.DTOs.Review;

public class ProductTrustBadgeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid TrustBadgeId { get; set; }
    public TrustBadgeDto TrustBadge { get; set; } = null!;
    public DateTime AwardedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string? AwardReason { get; set; }
}
