namespace Merge.Domain.Entities;

/// <summary>
/// Currency Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty; // USD, EUR, TRY, GBP
    public string Name { get; set; } = string.Empty; // US Dollar, Euro, Turkish Lira
    public string Symbol { get; set; } = string.Empty; // $, €, ₺, £
    public decimal ExchangeRate { get; set; } = 1.0m; // Rate relative to base currency
    public bool IsBaseCurrency { get; set; } = false; // Base currency has rate = 1.0
    public bool IsActive { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public int DecimalPlaces { get; set; } = 2;
    public string Format { get; set; } = string.Empty; // {symbol}{amount} or {amount}{symbol}
}

