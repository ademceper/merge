namespace Merge.Domain.Entities;

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

public class UserCurrencyPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
