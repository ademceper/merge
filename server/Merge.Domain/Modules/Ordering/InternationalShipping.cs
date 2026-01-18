using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// InternationalShipping Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class InternationalShipping : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string OriginCountry { get; private set; } = string.Empty;
    public string DestinationCountry { get; private set; } = string.Empty;
    public string? OriginCity { get; private set; }
    public string? DestinationCity { get; private set; }
    public string ShippingMethod { get; private set; } = string.Empty; // Express, Standard, Economy
    
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
    
    [NotMapped]
    public Money ShippingCostMoney => new Money(_shippingCost);
    
    [NotMapped]
    public Money? CustomsDutyMoney => _customsDuty.HasValue ? new Money(_customsDuty.Value) : null;
    
    [NotMapped]
    public Money? ImportTaxMoney => _importTax.HasValue ? new Money(_importTax.Value) : null;
    
    [NotMapped]
    public Money? HandlingFeeMoney => _handlingFee.HasValue ? new Money(_handlingFee.Value) : null;
    
    [NotMapped]
    public Money TotalCostMoney => new Money(_totalCost);
    
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
    
    public ShippingStatus Status { get; private set; } = ShippingStatus.Preparing;
    
    public DateTime? ShippedAt { get; private set; }
    public DateTime? InCustomsAt { get; private set; }
    public DateTime? ClearedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
    
    private InternationalShipping() { }
    
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
        
        internationalShipping.AddDomainEvent(new InternationalShippingCreatedEvent(
            internationalShipping.Id, 
            orderId, 
            originCountry, 
            destinationCountry, 
            shippingMethod, 
            shippingCost.Amount));
        
        return internationalShipping;
    }
    
    public void UpdateCustomsDuty(Money customsDuty)
    {
        Guard.AgainstNull(customsDuty, nameof(customsDuty));
        Guard.AgainstNegative(customsDuty.Amount, nameof(customsDuty));
        
        _customsDuty = customsDuty.Amount;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "CustomsDuty"));
    }
    
    public void UpdateImportTax(Money importTax)
    {
        Guard.AgainstNull(importTax, nameof(importTax));
        Guard.AgainstNegative(importTax.Amount, nameof(importTax));
        
        _importTax = importTax.Amount;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "ImportTax"));
    }
    
    public void UpdateHandlingFee(Money handlingFee)
    {
        Guard.AgainstNull(handlingFee, nameof(handlingFee));
        Guard.AgainstNegative(handlingFee.Amount, nameof(handlingFee));
        
        _handlingFee = handlingFee.Amount;
        RecalculateTotalCost();
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "HandlingFee"));
    }
    
    public void UpdateTrackingNumber(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        // Configuration değeri: MaxTrackingNumberLength=100
        Guard.AgainstLength(trackingNumber, 100, nameof(trackingNumber));
        
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingTrackingUpdatedEvent(Id, OrderId, trackingNumber));
    }
    
    public void UpdateCustomsDeclarationNumber(string customsDeclarationNumber)
    {
        Guard.AgainstNullOrEmpty(customsDeclarationNumber, nameof(customsDeclarationNumber));
        // Configuration değeri: MaxCustomsDeclarationNumberLength=100
        Guard.AgainstLength(customsDeclarationNumber, 100, nameof(customsDeclarationNumber));
        
        CustomsDeclarationNumber = customsDeclarationNumber;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "CustomsDeclarationNumber"));
    }
    
    public void UpdateEstimatedDays(int estimatedDays)
    {
        Guard.AgainstNegative(estimatedDays, nameof(estimatedDays));
        
        _estimatedDays = estimatedDays;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingUpdatedEvent(Id, OrderId, "EstimatedDays"));
    }
    
    public void MarkAsShipped(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        // Configuration değeri: MaxTrackingNumberLength=100
        Guard.AgainstLength(trackingNumber, 100, nameof(trackingNumber));
        
        if (Status != ShippingStatus.Preparing)
            throw new DomainException("Sadece hazırlanmakta olan uluslararası kargolar kargoya verilebilir");
        
        TrackingNumber = trackingNumber;
        Status = ShippingStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingStatusChangedEvent(Id, OrderId, ShippingStatus.Preparing, Status));
    }
    
    public void MarkAsInCustoms()
    {
        if (Status != ShippingStatus.Shipped && Status != ShippingStatus.InTransit)
            throw new DomainException("Sadece kargoya verilmiş veya yolda olan kargolar gümrükte olarak işaretlenebilir");
        
        InCustomsAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingInCustomsEvent(Id, OrderId));
    }
    
    public void MarkAsClearedFromCustoms()
    {
        if (InCustomsAt is null)
            throw new DomainException("Gümrükte olmayan kargo gümrükten çıkmış olarak işaretlenemez");
        
        ClearedAt = DateTime.UtcNow;
        Status = ShippingStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingClearedFromCustomsEvent(Id, OrderId));
    }
    
    public void MarkAsDelivered()
    {
        if (Status != ShippingStatus.InTransit && Status != ShippingStatus.OutForDelivery)
            throw new DomainException("Sadece yolda veya teslimata çıkmış kargolar teslim edilebilir");
        
        var oldStatus = Status;
        Status = ShippingStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingStatusChangedEvent(Id, OrderId, oldStatus, Status));
    }
    
    private void RecalculateTotalCost()
    {
        _totalCost = _shippingCost 
            + (_customsDuty ?? 0) 
            + (_importTax ?? 0) 
            + (_handlingFee ?? 0);
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;
        
        if (Status == ShippingStatus.Delivered)
            throw new DomainException("Teslim edilmiş kargo silinemez");
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new InternationalShippingDeletedEvent(Id, OrderId));
    }
}

