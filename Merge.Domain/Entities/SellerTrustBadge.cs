namespace Merge.Domain.Entities;

/// <summary>
/// SellerTrustBadge Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

