using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.ValueObjects;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// TaxRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TaxRule : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Country { get; private set; } = string.Empty;
    public string? State { get; private set; } // For countries with state-level tax
    public string? City { get; private set; } // For cities with local tax
    
    // ✅ BOLUM 1.2: Enum kullanımı (string TaxType YASAK)
    public TaxType TaxType { get; private set; }
    
    // ✅ BOLUM 1.6: Invariant validation - TaxRate 0-100 arası
    private decimal _taxRate;
    public decimal TaxRate 
    { 
        get => _taxRate; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0m, 100m, nameof(TaxRate));
            _taxRate = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public DateTime? EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string? ProductCategoryIds { get; private set; } // Comma separated category IDs (null = all categories)
    public bool IsInclusive { get; private set; } = false; // Tax included in price or added on top
    public string? Notes { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TaxRule() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static TaxRule Create(
        string country,
        TaxType taxType,
        decimal taxRate,
        string? state = null,
        string? city = null,
        bool isActive = true,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null,
        string? productCategoryIds = null,
        bool isInclusive = false,
        string? notes = null)
    {
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstOutOfRange(taxRate, 0m, 100m, nameof(taxRate));

        // ✅ BOLUM 1.6: Invariant validation
        if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveTo.Value < effectiveFrom.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        var taxRule = new TaxRule
        {
            Id = Guid.NewGuid(),
            Country = country,
            State = state,
            City = city,
            TaxType = taxType,
            _taxRate = taxRate,
            IsActive = isActive,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            ProductCategoryIds = productCategoryIds,
            IsInclusive = isInclusive,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - TaxRuleCreatedEvent
        taxRule.AddDomainEvent(new TaxRuleCreatedEvent(
            taxRule.Id,
            country,
            taxType,
            taxRate,
            state,
            city));

        return taxRule;
    }

    // ✅ BOLUM 1.1: Domain Method - Update tax rule
    public void Update(
        string? country = null,
        TaxType? taxType = null,
        decimal? taxRate = null,
        string? state = null,
        string? city = null,
        bool? isActive = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null,
        string? productCategoryIds = null,
        bool? isInclusive = null,
        string? notes = null)
    {
        if (!string.IsNullOrEmpty(country))
            Country = country;

        if (taxType.HasValue)
            TaxType = taxType.Value;

        if (taxRate.HasValue)
        {
            Guard.AgainstOutOfRange(taxRate.Value, 0m, 100m, nameof(taxRate));
            _taxRate = taxRate.Value;
        }

        if (state != null)
            State = state;

        if (city != null)
            City = city;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        if (effectiveFrom.HasValue)
            EffectiveFrom = effectiveFrom;

        if (effectiveTo.HasValue)
            EffectiveTo = effectiveTo;

        if (productCategoryIds != null)
            ProductCategoryIds = productCategoryIds;

        if (isInclusive.HasValue)
            IsInclusive = isInclusive.Value;

        if (notes != null)
            Notes = notes;

        // ✅ BOLUM 1.6: Invariant validation
        if (EffectiveFrom.HasValue && EffectiveTo.HasValue && EffectiveTo.Value < EffectiveFrom.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TaxRuleUpdatedEvent
        AddDomainEvent(new TaxRuleUpdatedEvent(Id, Country, TaxType, _taxRate));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate tax rule
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TaxRuleActivatedEvent
        AddDomainEvent(new TaxRuleActivatedEvent(Id, Country, TaxType));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate tax rule
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TaxRuleDeactivatedEvent
        AddDomainEvent(new TaxRuleDeactivatedEvent(Id, Country, TaxType));
    }

    // ✅ BOLUM 1.1: Domain Method - Check if tax rule is effective
    public bool IsEffective(DateTime? date = null)
    {
        var checkDate = date ?? DateTime.UtcNow;

        if (!IsActive)
            return false;

        if (EffectiveFrom.HasValue && checkDate < EffectiveFrom.Value)
            return false;

        if (EffectiveTo.HasValue && checkDate > EffectiveTo.Value)
            return false;

        return true;
    }

    // ✅ BOLUM 1.1: Domain Method - Calculate tax amount
    public decimal CalculateTax(decimal baseAmount)
    {
        Guard.AgainstNegativeOrZero(baseAmount, nameof(baseAmount));

        if (!IsEffective())
            throw new DomainException("Vergi kuralı geçerli değil");

        return Math.Round(baseAmount * (_taxRate / 100), 2);
    }

    // ✅ BOLUM 1.1: Domain Method - Delete (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Tax rule zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TaxRuleDeletedEvent
        AddDomainEvent(new TaxRuleDeletedEvent(Id, Country, TaxType));
    }
}

