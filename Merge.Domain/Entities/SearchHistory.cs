namespace Merge.Domain.Entities;

/// <summary>
/// SearchHistory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SearchHistory : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable for anonymous users
    public string SearchTerm { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public bool ClickedResult { get; set; } = false;
    public Guid? ClickedProductId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Product? ClickedProduct { get; set; }
}

