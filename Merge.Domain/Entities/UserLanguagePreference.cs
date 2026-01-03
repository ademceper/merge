namespace Merge.Domain.Entities;

/// <summary>
/// UserLanguagePreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserLanguagePreference : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

