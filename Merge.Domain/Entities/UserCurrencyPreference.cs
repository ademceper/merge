namespace Merge.Domain.Entities;

/// <summary>
/// UserCurrencyPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserCurrencyPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}

