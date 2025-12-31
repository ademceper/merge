namespace Merge.Domain.Entities;

public class TrustBadge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string BadgeType { get; set; } = string.Empty; // Seller, Product, Order
    public string Criteria { get; set; } = string.Empty; // JSON formatında kriterler
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string? Color { get; set; } // Hex color code
}

public class SellerTrustBadge : BaseEntity
{
    public Guid SellerId { get; set; }
    public Guid TrustBadgeId { get; set; }
    public DateTime AwardedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AwardReason { get; set; }
    
    // Navigation properties
    public SellerProfile Seller { get; set; } = null!;
    public TrustBadge TrustBadge { get; set; } = null!;
}

public class ProductTrustBadge : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid TrustBadgeId { get; set; }
    public DateTime AwardedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AwardReason { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public TrustBadge TrustBadge { get; set; } = null!;
}

