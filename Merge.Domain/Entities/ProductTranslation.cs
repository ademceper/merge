namespace Merge.Domain.Entities;

/// <summary>
/// ProductTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductTranslation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

