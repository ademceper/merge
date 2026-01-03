namespace Merge.Domain.Entities;

/// <summary>
/// TrustBadge Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

