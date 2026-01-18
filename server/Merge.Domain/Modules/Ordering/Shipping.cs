using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// Shipping Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Shipping : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string ShippingProvider { get; private set; } = string.Empty;
    public string TrackingNumber { get; private set; } = string.Empty;
    
    public ShippingStatus Status { get; private set; } = ShippingStatus.Preparing;
    
    public DateTime? ShippedDate { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    
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
    
    public string? ShippingLabelUrl { get; private set; }

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

    [NotMapped]
    public Money ShippingCostMoney => new Money(_shippingCost);

    // Navigation properties
    public Order Order { get; private set; } = null!;

    private Shipping() { }

    public static Shipping Create(
        Guid orderId,
        string shippingProvider,
        Money shippingCost,
        DateTime? estimatedDeliveryDate = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(shippingProvider, nameof(shippingProvider));
        Guard.AgainstNull(shippingCost, nameof(shippingCost));

        var shipping = new Shipping
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ShippingProvider = shippingProvider,
            _shippingCost = shippingCost.Amount,
            Status = ShippingStatus.Preparing,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            CreatedAt = DateTime.UtcNow
        };

        shipping.AddDomainEvent(new ShippingCreatedEvent(shipping.Id, orderId, shippingProvider, shippingCost.Amount));

        return shipping;
    }

    private static readonly Dictionary<ShippingStatus, ShippingStatus[]> AllowedTransitions = new()
    {
        { ShippingStatus.Preparing, new[] { ShippingStatus.Shipped } },
        { ShippingStatus.Shipped, new[] { ShippingStatus.InTransit } },
        { ShippingStatus.InTransit, new[] { ShippingStatus.OutForDelivery, ShippingStatus.Returned, ShippingStatus.Failed } },
        { ShippingStatus.OutForDelivery, new[] { ShippingStatus.Delivered, ShippingStatus.Returned, ShippingStatus.Failed } },
        { ShippingStatus.Delivered, Array.Empty<ShippingStatus>() }, // Terminal state
        { ShippingStatus.Returned, Array.Empty<ShippingStatus>() }, // Terminal state
        { ShippingStatus.Failed, Array.Empty<ShippingStatus>() } // Terminal state
    };

    public void TransitionTo(ShippingStatus newStatus)
    {
        if (!AllowedTransitions.ContainsKey(Status))
            throw new InvalidStateTransitionException(Status, newStatus);

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidStateTransitionException(Status, newStatus);

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        // Set dates based on status
        if (newStatus == ShippingStatus.Shipped)
            ShippedDate = DateTime.UtcNow;
        else if (newStatus == ShippingStatus.Delivered)
            DeliveredDate = DateTime.UtcNow;

        AddDomainEvent(new ShippingStatusChangedEvent(Id, OrderId, oldStatus, newStatus));
    }

    public void Ship(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));

        if (Status != ShippingStatus.Preparing)
            throw new DomainException("Sadece hazırlanmakta olan kargolar kargoya verilebilir");

        TrackingNumber = trackingNumber;
        TransitionTo(ShippingStatus.Shipped);
    }

    public void MarkInTransit()
    {
        if (Status != ShippingStatus.Shipped)
            throw new DomainException("Sadece kargoya verilmiş kargolar yolda olarak işaretlenebilir");

        TransitionTo(ShippingStatus.InTransit);
    }

    public void MarkOutForDelivery()
    {
        if (Status != ShippingStatus.InTransit)
            throw new DomainException("Sadece yolda olan kargolar teslimata çıkmış olarak işaretlenebilir");

        TransitionTo(ShippingStatus.OutForDelivery);
    }

    public void Deliver()
    {
        if (Status != ShippingStatus.OutForDelivery && Status != ShippingStatus.InTransit)
            throw new DomainException("Sadece teslimata çıkmış veya yolda olan kargolar teslim edilebilir");

        TransitionTo(ShippingStatus.Delivered);
    }

    public void Return(string? reason = null)
    {
        if (Status != ShippingStatus.InTransit && Status != ShippingStatus.OutForDelivery)
            throw new DomainException("Sadece yolda veya teslimata çıkmış kargolar iade edilebilir");

        TransitionTo(ShippingStatus.Returned);
    }

    public void MarkAsFailed(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (Status != ShippingStatus.InTransit && Status != ShippingStatus.OutForDelivery)
            throw new DomainException("Sadece yolda veya teslimata çıkmış kargolar başarısız olarak işaretlenebilir");

        TransitionTo(ShippingStatus.Failed);
    }

    public void UpdateTrackingNumber(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingTrackingUpdatedEvent(Id, OrderId, trackingNumber));
    }

    public void UpdateEstimatedDeliveryDate(DateTime estimatedDeliveryDate)
    {
        if (estimatedDeliveryDate <= DateTime.UtcNow)
            throw new DomainException("Tahmini teslimat tarihi gelecekte olmalıdır");

        EstimatedDeliveryDate = estimatedDeliveryDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingDetailsUpdatedEvent(Id, OrderId, "EstimatedDeliveryDate"));
    }

    public void SetShippingLabelUrl(string url)
    {
        Guard.AgainstNullOrEmpty(url, nameof(url));
        ShippingLabelUrl = url;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingDetailsUpdatedEvent(Id, OrderId, "ShippingLabelUrl"));
    }

    public void UpdateShippingCost(Money newCost)
    {
        Guard.AgainstNull(newCost, nameof(newCost));
        Guard.AgainstNegative(newCost.Amount, nameof(newCost));

        if (Status != ShippingStatus.Preparing)
            throw new DomainException("Sadece hazırlanmakta olan kargoların maliyeti güncellenebilir");

        _shippingCost = newCost.Amount;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShippingDetailsUpdatedEvent(Id, OrderId, "ShippingCost"));
    }
}

