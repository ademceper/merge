namespace Merge.Domain.Entities;

/// <summary>
/// SharedWishlistItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SharedWishlistItem : BaseEntity
{
    public Guid SharedWishlistId { get; set; }
    public Guid ProductId { get; set; }
    public int Priority { get; set; } = 0; // 1 = High, 2 = Medium, 3 = Low
    public string Note { get; set; } = string.Empty;
    public bool IsPurchased { get; set; } = false;
    public Guid? PurchasedBy { get; set; }
    public DateTime? PurchasedAt { get; set; }

    // Navigation properties
    public SharedWishlist SharedWishlist { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public User? PurchasedByUser { get; set; }
}

