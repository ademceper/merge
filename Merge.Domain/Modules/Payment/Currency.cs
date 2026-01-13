using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// Currency Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Currency : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Code { get; private set; } = string.Empty; // USD, EUR, TRY, GBP
    public string Name { get; private set; } = string.Empty; // US Dollar, Euro, Turkish Lira
    public string Symbol { get; private set; } = string.Empty; // $, €, ₺, £
    
    private decimal _exchangeRate = 1.0m;
    public decimal ExchangeRate 
    { 
        get => _exchangeRate; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ExchangeRate));
            _exchangeRate = value;
        } 
    } // Rate relative to base currency
    
    public bool IsBaseCurrency { get; private set; } = false; // Base currency has rate = 1.0
    public bool IsActive { get; private set; } = true;
    public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;
    
    private int _decimalPlaces = 2;
    public int DecimalPlaces 
    { 
        get => _decimalPlaces; 
        private set 
        { 
            Guard.AgainstOutOfRange(value, 0, 10, nameof(DecimalPlaces));
            _decimalPlaces = value;
        } 
    }
    
    public string Format { get; private set; } = string.Empty; // {symbol}{amount} or {amount}{symbol}

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Currency() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Currency Create(
        string code,
        string name,
        string symbol,
        decimal exchangeRate = 1.0m,
        bool isBaseCurrency = false,
        bool isActive = true,
        int decimalPlaces = 2,
        string format = "{symbol}{amount}")
    {
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
        Guard.AgainstNegative(exchangeRate, nameof(exchangeRate));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinCurrencyDecimalPlaces=0, MaxCurrencyDecimalPlaces=10
        Guard.AgainstOutOfRange(decimalPlaces, 0, 10, nameof(decimalPlaces));

        if (isBaseCurrency && exchangeRate != 1.0m)
            throw new DomainException("Base currency must have exchange rate of 1.0");

        var currency = new Currency
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Name = name,
            Symbol = symbol,
            _exchangeRate = exchangeRate,
            IsBaseCurrency = isBaseCurrency,
            IsActive = isActive,
            _decimalPlaces = decimalPlaces,
            Format = format,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CurrencyCreatedEvent
        currency.AddDomainEvent(new CurrencyCreatedEvent(currency.Id, currency.Code, currency.Name));

        return currency;
    }

    // ✅ BOLUM 1.1: Domain Method - Update currency details
    public void UpdateDetails(string name, string symbol, int decimalPlaces, string format)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinCurrencyDecimalPlaces=0, MaxCurrencyDecimalPlaces=10
        Guard.AgainstOutOfRange(decimalPlaces, 0, 10, nameof(decimalPlaces));

        Name = name;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
        Format = format;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencyUpdatedEvent
        AddDomainEvent(new CurrencyUpdatedEvent(Id, Code, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update exchange rate
    public void UpdateExchangeRate(decimal newRate, string source = "Manual")
    {
        if (IsBaseCurrency)
            throw new DomainException("Base currency exchange rate cannot be updated");

        Guard.AgainstNegative(newRate, nameof(newRate));

        var oldRate = _exchangeRate;
        _exchangeRate = newRate;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencyExchangeRateUpdatedEvent
        AddDomainEvent(new CurrencyExchangeRateUpdatedEvent(Id, Code, oldRate, newRate, source));
    }

    // ✅ BOLUM 1.1: Domain Method - Set as base currency
    public void SetAsBaseCurrency()
    {
        if (IsBaseCurrency)
            return;

        _exchangeRate = 1.0m;
        IsBaseCurrency = true;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencySetAsBaseEvent
        AddDomainEvent(new CurrencySetAsBaseEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Remove base currency status
    public void RemoveBaseCurrencyStatus()
    {
        if (!IsBaseCurrency)
            return;

        IsBaseCurrency = false;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CurrencyBaseCurrencyStatusRemovedEvent
        AddDomainEvent(new CurrencyBaseCurrencyStatusRemovedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate currency
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencyActivatedEvent
        AddDomainEvent(new CurrencyActivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate currency
    public void Deactivate()
    {
        if (!IsActive)
            return;

        if (IsBaseCurrency)
            throw new DomainException("Base currency cannot be deactivated");

        IsActive = false;
        LastUpdated = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencyDeactivatedEvent
        AddDomainEvent(new CurrencyDeactivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        if (IsBaseCurrency)
            throw new DomainException("Base currency cannot be deleted");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CurrencyDeletedEvent
        AddDomainEvent(new CurrencyDeletedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Restore currency
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CurrencyRestoredEvent
        AddDomainEvent(new CurrencyRestoredEvent(Id, Code));
    }
}

