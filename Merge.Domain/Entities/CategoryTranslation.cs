namespace Merge.Domain.Entities;

/// <summary>
/// CategoryTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CategoryTranslation : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

