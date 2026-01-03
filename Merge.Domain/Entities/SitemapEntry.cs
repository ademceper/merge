namespace Merge.Domain.Entities;

/// <summary>
/// SitemapEntry Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SitemapEntry : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty; // Product, Category, Blog, Page
    public Guid? EntityId { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ChangeFrequency { get; set; } = "weekly"; // always, hourly, daily, weekly, monthly, yearly, never
    public decimal Priority { get; set; } = 0.5m; // 0.0 to 1.0
    public bool IsActive { get; set; } = true;
}

