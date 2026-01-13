using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// InternationalShipping Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class InternationalShipping : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public string OriginCountry { get; private set; } = string.Empty;
    public string DestinationCountry { get; private set; } = string.Empty;
    public string? OriginCity { get; private set; }
    public string? DestinationCity { get; private set; }
    public string ShippingMethod { get; private set; } = string.Empty; // Express, Standard, Economy
    
    // ✅ BOLUM 1.1: Rich Domain Model - Private backing fields with validation
    private decimal _shippingCost;
    public decimal ShippingCost 
    { 
        get => _shippingCost; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ShippingCost));
            _shippingCost = value;
        } 
    }
    
    private decimal? _customsDuty;
    public decimal? CustomsDuty 
    { 
        get => _customsDuty; 
        private set 
        { 
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(CustomsDuty));
            _customsDuty = value;
        } 
    }
    
    private decimal? _importTax;
    public decimal? ImportTax 
    { 
        get => _importTax; 
        private set 
        { 
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(ImportTax));
            _importTax = value;
        } 
    }
    
    private decimal? _handlingFee;
    public decimal? HandlingFee 
    { 
        get => _handlingFee; 
        private set 
        { 
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(HandlingFee));
            _handlingFee = value;
        } 
    }
    
    private decimal _totalCost;
    public decimal TotalCost 
    { 
        get => _totalCost; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(TotalCost));
            _totalCost = value;
        } 
    }
    
    private int _estimatedDays;
    public int EstimatedDays 
    { 
        get => _estimatedDays; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(EstimatedDays));
            _estimatedDays = value;
        } 
    }
    
    public string? TrackingNumber { get; private set; }
    public string? CustomsDeclarationNumber { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public ShippingStatus Status { get; private set; } = ShippingStatus.Preparing;
    
    public DateTime? ShippedAt { get; private set; }
    public DateTime? InCustomsAt { get; private set; }
    public DateTime? ClearedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Order Order { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private InternationalShipping() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static InternationalShipping Create(
        Guid orderId,
        string originCountry,
        string destinationCountry,
        string shippingMethod,
        Money shippingCost,
        int estimatedDays,
        string? originCity = null,
        string? destinationCity = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(originCountry, nameof(originCountry));
        Guard.AgainstNullOrEmpty(destinationCountry, nameof(destinationCountry));
        Guard.AgainstNullOrEmpty(shippingMethod, nameof(shippingMethod));
        Guard.AgainstNull(shippingCost, nameof(shippingCost));
        Guard.AgainstNegative(estimatedDays, nameof(estimatedDays));
        
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxCountryNameLength=100, MaxCityNameLength=100, MaxShippingMethodLength=50
        Guard.AgainstLength(originCountry, 100, nameof(originCountry));
        Guard.AgainstLength(destinationCountry, 100, nameof(destinationCountry));
        Guard.AgainstLength(shippingMethod, 50, nameof(shippingMethod));
        
        if (!string.IsNullOrEmpty(originCity))
            Guard.AgainstLength(originCity, 100, nameof(originCity));
        if (!string.IsNullOrEmpty(destinationCity))
            Guard.AgainstLength(destinationCity, 100, nameof(destinationCity));
        
        var internationalShipping = new InternationalShipping
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            OriginCountry = originCountry,
            DestinationCountry = destinationCountry,
            OriginCity = originCity,
            DestinationCity = destinationCity,
            ShippingMethod = shippingMethod,
            _shippingCost = shippingCost.Amount,
            _totalCost = shippingCost.Amount, // Initially total = shipping cost
            _estimatedDays = estimatedDays,
            Status = ShippingStatus.Preparing,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingCreatedEvent
        internationalShipping.AddDomainEvent(new InternationalShippingCreatedEvent(
            internationalShipping.Id, 
            orderId, 
            originCountry, 
            destinationCountry, 
            shippingMethod, 
            shippingCost.Amount));
        
        return internationalShipping;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update customs duty
    public void UpdateCustomsDuty(decimal customsDuty)
    {
        Guard.AgainstNegative(customsDuty, nameof(customsDuty));
        
        _customsDuty = customsDuty;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingUpdatedEvent
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "CustomsDuty"));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update import tax
    public void UpdateImportTax(decimal importTax)
    {
        Guard.AgainstNegative(importTax, nameof(importTax));
        
        _importTax = importTax;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingUpdatedEvent
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "ImportTax"));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update handling fee
    public void UpdateHandlingFee(decimal handlingFee)
    {
        Guard.AgainstNegative(handlingFee, nameof(handlingFee));
        
        _handlingFee = handlingFee;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingUpdatedEvent
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "HandlingFee"));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update tracking number
    public void UpdateTrackingNumber(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTrackingNumberLength=100
        Guard.AgainstLength(trackingNumber, 100, nameof(trackingNumber));
        
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingTrackingUpdatedEvent
        AddDomainEvent(new InternationalShippingTrackingUpdatedEvent(Id, OrderId, trackingNumber));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update customs declaration number
    public void UpdateCustomsDeclarationNumber(string customsDeclarationNumber)
    {
        Guard.AgainstNullOrEmpty(customsDeclarationNumber, nameof(customsDeclarationNumber));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCustomsDeclarationNumberLength=100
        Guard.AgainstLength(customsDeclarationNumber, 100, nameof(customsDeclarationNumber));
        
        CustomsDeclarationNumber = customsDeclarationNumber;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingUpdatedEvent
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "CustomsDeclarationNumber"));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update estimated days
    public void UpdateEstimatedDays(int estimatedDays)
    {
        Guard.AgainstNegative(estimatedDays, nameof(estimatedDays));
        
        _estimatedDays = estimatedDays;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingUpdatedEvent
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "EstimatedDays"));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as shipped
    public void MarkAsShipped(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTrackingNumberLength=100
        Guard.AgainstLength(trackingNumber, 100, nameof(trackingNumber));
        
        if (Status != ShippingStatus.Preparing)
            throw new DomainException("Sadece hazırlanmakta olan uluslararası kargolar kargoya verilebilir");
        
        TrackingNumber = trackingNumber;
        Status = ShippingStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingStatusChangedEvent
        AddDomainEvent(new InternationalShippingStatusChangedEvent(Id, OrderId, ShippingStatus.Preparing, Status));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as in customs
    public void MarkAsInCustoms()
    {
        if (Status != ShippingStatus.Shipped && Status != ShippingStatus.InTransit)
            throw new DomainException("Sadece kargoya verilmiş veya yolda olan kargolar gümrükte olarak işaretlenebilir");
        
        InCustomsAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingInCustomsEvent
        AddDomainEvent(new InternationalShippingInCustomsEvent(Id, OrderId));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as cleared from customs
    public void MarkAsClearedFromCustoms()
    {
        if (InCustomsAt == null)
            throw new DomainException("Gümrükte olmayan kargo gümrükten çıkmış olarak işaretlenemez");
        
        ClearedAt = DateTime.UtcNow;
        Status = ShippingStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingClearedFromCustomsEvent
        AddDomainEvent(new InternationalShippingClearedFromCustomsEvent(Id, OrderId));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as delivered
    public void MarkAsDelivered()
    {
        if (Status != ShippingStatus.InTransit && Status != ShippingStatus.OutForDelivery)
            throw new DomainException("Sadece yolda veya teslimata çıkmış kargolar teslim edilebilir");
        
        var oldStatus = Status;
        Status = ShippingStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingStatusChangedEvent
        AddDomainEvent(new InternationalShippingStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }
    
    // ✅ BOLUM 1.1: Domain Method - Recalculate total cost
    private void RecalculateTotalCost()
    {
        _totalCost = _shippingCost 
            + (_customsDuty ?? 0) 
            + (_importTax ?? 0) 
            + (_handlingFee ?? 0);
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;
        
        if (Status == ShippingStatus.Delivered)
            throw new DomainException("Teslim edilmiş kargo silinemez");
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - InternationalShippingDeletedEvent
        AddDomainEvent(new InternationalShippingDeletedEvent(Id, OrderId));
    }
}

