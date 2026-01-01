using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;

namespace Merge.Domain.Entities;

/// <summary>
/// Shipping aggregate root - Rich Domain Model implementation
/// </summary>
public class Shipping : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public string ShippingProvider { get; private set; } = string.Empty;
    public string TrackingNumber { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public ShippingStatus Status { get; private set; } = ShippingStatus.Preparing;
    
    public DateTime? ShippedDate { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money
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

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.3: Value Object property
    [NotMapped]
    public Money ShippingCostMoney => new Money(_shippingCost);

    // Navigation properties
    public Order Order { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Shipping() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        return shipping;
    }

    // ✅ BOLUM 1.1: State Machine Pattern - Transition to new status
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

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        // Set dates based on status
        if (newStatus == ShippingStatus.Shipped)
            ShippedDate = DateTime.UtcNow;
        else if (newStatus == ShippingStatus.Delivered)
            DeliveredDate = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Convenience methods for common transitions
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

    // ✅ BOLUM 1.1: Domain Logic - Update tracking number
    public void UpdateTrackingNumber(string trackingNumber)
    {
        Guard.AgainstNullOrEmpty(trackingNumber, nameof(trackingNumber));
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update estimated delivery date
    public void UpdateEstimatedDeliveryDate(DateTime estimatedDeliveryDate)
    {
        if (estimatedDeliveryDate <= DateTime.UtcNow)
            throw new DomainException("Tahmini teslimat tarihi gelecekte olmalıdır");

        EstimatedDeliveryDate = estimatedDeliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set shipping label URL
    public void SetShippingLabelUrl(string url)
    {
        Guard.AgainstNullOrEmpty(url, nameof(url));
        ShippingLabelUrl = url;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update shipping cost
    public void UpdateShippingCost(Money newCost)
    {
        Guard.AgainstNull(newCost, nameof(newCost));
        Guard.AgainstNegative(newCost.Amount, nameof(newCost));

        if (Status != ShippingStatus.Preparing)
            throw new DomainException("Sadece hazırlanmakta olan kargoların maliyeti güncellenebilir");

        _shippingCost = newCost.Amount;
        UpdatedAt = DateTime.UtcNow;
    }
}

