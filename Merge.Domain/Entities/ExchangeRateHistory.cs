namespace Merge.Domain.Entities;

/// <summary>
/// ExchangeRateHistory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ExchangeRateHistory : BaseEntity
{
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty; // Manual, API, etc.

    // Navigation properties
    public Currency Currency { get; set; } = null!;
}

