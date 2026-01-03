namespace Merge.Domain.Entities;

/// <summary>
/// LiveStreamProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamProduct : BaseEntity
{
    public Guid LiveStreamId { get; set; }
    public LiveStream LiveStream { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int DisplayOrder { get; set; } = 0;
    public bool IsHighlighted { get; set; } = false; // Currently showcased
    public DateTime? ShowcasedAt { get; set; } // When product was showcased
    public int ViewCount { get; set; } = 0; // Views during showcase
    public int ClickCount { get; set; } = 0; // Clicks during showcase
    public int OrderCount { get; set; } = 0; // Orders during showcase
    public decimal? SpecialPrice { get; set; } // Special price during stream
    public string? ShowcaseNotes { get; set; } // Notes about the product showcase
}

