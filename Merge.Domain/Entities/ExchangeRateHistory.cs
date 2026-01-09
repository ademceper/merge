using Merge.Domain.Common;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// ExchangeRateHistory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ExchangeRateHistory : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CurrencyId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    
    private decimal _exchangeRate;
    public decimal ExchangeRate 
    { 
        get => _exchangeRate; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ExchangeRate));
            _exchangeRate = value;
        } 
    }
    
    public DateTime RecordedAt { get; private set; } = DateTime.UtcNow;
    public string Source { get; private set; } = string.Empty; // Manual, API, etc.

    // Navigation properties
    public Currency Currency { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ExchangeRateHistory() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static ExchangeRateHistory Create(
        Guid currencyId,
        string currencyCode,
        decimal exchangeRate,
        string source = "Manual")
    {
        Guard.AgainstDefault(currencyId, nameof(currencyId));
        Guard.AgainstNullOrEmpty(currencyCode, nameof(currencyCode));
        Guard.AgainstNegative(exchangeRate, nameof(exchangeRate));

        return new ExchangeRateHistory
        {
            Id = Guid.NewGuid(),
            CurrencyId = currencyId,
            CurrencyCode = currencyCode.ToUpperInvariant(),
            _exchangeRate = exchangeRate,
            RecordedAt = DateTime.UtcNow,
            Source = source,
            CreatedAt = DateTime.UtcNow
        };
    }
}

