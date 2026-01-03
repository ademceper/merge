namespace Merge.Domain.Entities;

/// <summary>
/// StaticTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class StaticTranslation : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "button.add_to_cart", "header.welcome"
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // UI, Email, Notification, etc.

    // Navigation properties
    public Language Language { get; set; } = null!;
}

