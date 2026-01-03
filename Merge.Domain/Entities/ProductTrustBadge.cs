namespace Merge.Domain.Entities;

/// <summary>
/// ProductTrustBadge Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

