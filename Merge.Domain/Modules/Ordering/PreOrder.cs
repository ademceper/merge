using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// PreOrder Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class PreOrder : BaseAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public int Quantity { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing fields (EF Core compatibility)
    private decimal _price;
    public decimal Price
    {
        get => _price;
        private set
        {
            if (value < 0)
                throw new DomainException("Fiyat negatif olamaz");
            _price = value;
        }
    }
    
    private decimal _depositAmount;
    public decimal DepositAmount
    {
        get => _depositAmount;
        private set
        {
            if (value < 0)
                throw new DomainException("Depozito tutarı negatif olamaz");
            _depositAmount = value;
        }
    }
    
    private decimal _depositPaid;
    public decimal DepositPaid
    {
        get => _depositPaid;
        private set
        {
            if (value < 0)
                throw new DomainException("Ödenen depozito negatif olamaz");
            if (value > DepositAmount)
                throw new DomainException("Ödenen depozito toplam depozito tutarını aşamaz");
            _depositPaid = value;
        }
    }
    
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public PreOrderStatus Status { get; private set; }
    
    public DateTime ExpectedAvailabilityDate { get; private set; }
    public DateTime? ActualAvailabilityDate { get; private set; }
    public DateTime? NotificationSentAt { get; private set; }
    public DateTime? ConvertedToOrderAt { get; private set; }
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? Notes { get; private set; }
    public string? VariantOptions { get; private set; } // JSON for selected variant options

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PreOrder() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PreOrder Create(
        Guid userId,
        Guid productId,
        int quantity,
        decimal price,
        decimal depositAmount,
        DateTime expectedAvailabilityDate,
        DateTime expiresAt,
        string? notes = null,
        string? variantOptions = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegativeOrZero(price, nameof(price));
        Guard.AgainstNegative(depositAmount, nameof(depositAmount));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Son kullanma tarihi gelecekte olmalıdır");
        if (expectedAvailabilityDate <= DateTime.UtcNow)
            throw new DomainException("Beklenen teslimat tarihi gelecekte olmalıdır");
        if (depositAmount > price)
            throw new DomainException("Depozito tutarı fiyattan büyük olamaz");

        var preOrder = new PreOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            Price = price,
            DepositAmount = depositAmount,
            DepositPaid = 0,
            Status = PreOrderStatus.Pending,
            ExpectedAvailabilityDate = expectedAvailabilityDate,
            ExpiresAt = expiresAt,
            Notes = notes,
            VariantOptions = variantOptions,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - PreOrderCreatedEvent yayınla
        preOrder.AddDomainEvent(new PreOrderCreatedEvent(preOrder.Id, userId, productId, quantity, price));

        return preOrder;
    }

    // ✅ BOLUM 1.1: Domain Logic - Pay deposit (State Transition)
    public void PayDeposit(decimal amount)
    {
        if (Status != PreOrderStatus.Pending)
            throw new DomainException("Sadece bekleyen ön siparişler için depozito ödenebilir");

        if (amount <= 0)
            throw new DomainException("Depozito tutarı pozitif olmalıdır");

        if (DepositPaid + amount > DepositAmount)
            throw new DomainException("Toplam ödenen depozito, depozito tutarını aşamaz");

        DepositPaid += amount;

        if (DepositPaid >= DepositAmount)
        {
            Status = PreOrderStatus.DepositPaid;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Confirm pre-order (State Transition)
    public void Confirm()
    {
        if (Status != PreOrderStatus.DepositPaid)
            throw new DomainException("Sadece depozitosu ödenmiş ön siparişler onaylanabilir");

        Status = PreOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as ready to ship (State Transition)
    public void MarkAsReadyToShip()
    {
        if (Status != PreOrderStatus.Confirmed)
            throw new DomainException("Sadece onaylanmış ön siparişler sevkiyata hazır olarak işaretlenebilir");

        Status = PreOrderStatus.ReadyToShip;
        ActualAvailabilityDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Convert to order (State Transition)
    public void ConvertToOrder(Guid orderId)
    {
        if (Status != PreOrderStatus.ReadyToShip)
            throw new DomainException("Sadece sevkiyata hazır ön siparişler siparişe dönüştürülebilir");

        Status = PreOrderStatus.Converted;
        OrderId = orderId;
        ConvertedToOrderAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PreOrderConvertedEvent yayınla
        AddDomainEvent(new PreOrderConvertedEvent(Id, orderId, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Cancel pre-order (State Transition)
    public void Cancel()
    {
        if (Status == PreOrderStatus.Converted)
            throw new DomainException("Dönüştürülmüş ön siparişler iptal edilemez");
        if (Status == PreOrderStatus.Cancelled)
            throw new DomainException("Zaten iptal edilmiş ön sipariş");

        Status = PreOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PreOrderCancelledEvent yayınla
        AddDomainEvent(new PreOrderCancelledEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as expired
    public void MarkAsExpired()
    {
        if (Status == PreOrderStatus.Converted || Status == PreOrderStatus.Cancelled)
            return; // Already in final state

        if (DateTime.UtcNow < ExpiresAt)
            throw new DomainException("Ön sipariş henüz süresi dolmamış");

        Status = PreOrderStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark notification as sent
    public void MarkNotificationAsSent()
    {
        if (NotificationSentAt.HasValue)
            return; // Idempotent operation

        NotificationSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

