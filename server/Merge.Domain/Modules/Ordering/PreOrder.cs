using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PreOrder : BaseEntity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    private int _quantity;
    public int Quantity 
    { 
        get => _quantity; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Quantity));
            _quantity = value;
        }
    }
    
    private decimal _price;
    public decimal Price
    {
        get => _price;
        private set
        {
            Guard.AgainstNegativeOrZero(value, nameof(Price));
            _price = value;
        }
    }
    
    private decimal _depositAmount;
    public decimal DepositAmount
    {
        get => _depositAmount;
        private set
        {
            Guard.AgainstNegative(value, nameof(DepositAmount));
            _depositAmount = value;
        }
    }
    
    private decimal _depositPaid;
    public decimal DepositPaid
    {
        get => _depositPaid;
        private set
        {
            Guard.AgainstNegative(value, nameof(DepositPaid));
            if (value > _depositAmount)
                throw new DomainException("Ödenen depozito toplam depozito tutarını aşamaz");
            _depositPaid = value;
        }
    }
    
    [NotMapped]
    public Money PriceMoney => new Money(_price);
    
    [NotMapped]
    public Money DepositAmountMoney => new Money(_depositAmount);
    
    [NotMapped]
    public Money DepositPaidMoney => new Money(_depositPaid);
    
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

    private PreOrder() { }

    public static PreOrder Create(
        Guid userId,
        Guid productId,
        Product product,
        User user,
        int quantity,
        Money price,
        Money depositAmount,
        DateTime expectedAvailabilityDate,
        DateTime expiresAt,
        string? notes = null,
        string? variantOptions = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNull(user, nameof(user));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(price, nameof(price));
        Guard.AgainstNull(depositAmount, nameof(depositAmount));
        Guard.AgainstNegativeOrZero(price.Amount, nameof(price));
        Guard.AgainstNegative(depositAmount.Amount, nameof(depositAmount));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Son kullanma tarihi gelecekte olmalıdır");
        if (expectedAvailabilityDate <= DateTime.UtcNow)
            throw new DomainException("Beklenen teslimat tarihi gelecekte olmalıdır");
        if (depositAmount.Amount > price.Amount)
            throw new DomainException("Depozito tutarı fiyattan büyük olamaz");

        var preOrder = new PreOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Product = product,
            User = user,
            _quantity = quantity, // EF Core compatibility - backing field
            _price = price.Amount, // EF Core compatibility - backing field
            _depositAmount = depositAmount.Amount, // EF Core compatibility - backing field
            _depositPaid = 0, // EF Core compatibility - backing field
            Status = PreOrderStatus.Pending,
            ExpectedAvailabilityDate = expectedAvailabilityDate,
            ExpiresAt = expiresAt,
            Notes = notes,
            VariantOptions = variantOptions,
            CreatedAt = DateTime.UtcNow
        };

        preOrder.AddDomainEvent(new PreOrderCreatedEvent(preOrder.Id, userId, productId, quantity, price.Amount));

        return preOrder;
    }

    public void PayDeposit(Money amount)
    {
        Guard.AgainstNull(amount, nameof(amount));
        
        if (Status != PreOrderStatus.Pending)
            throw new DomainException("Sadece bekleyen ön siparişler için depozito ödenebilir");

        if (amount.Amount <= 0)
            throw new DomainException("Depozito tutarı pozitif olmalıdır");

        if (_depositPaid + amount.Amount > _depositAmount)
            throw new DomainException("Toplam ödenen depozito, depozito tutarını aşamaz");

        var wasDepositPaid = _depositPaid >= _depositAmount;
        _depositPaid += amount.Amount;

        if (_depositPaid >= _depositAmount && !wasDepositPaid)
        {
            Status = PreOrderStatus.DepositPaid;
            
            AddDomainEvent(new PreOrderDepositPaidEvent(Id, UserId, _depositAmount));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != PreOrderStatus.DepositPaid)
            throw new DomainException("Sadece depozitosu ödenmiş ön siparişler onaylanabilir");

        Status = PreOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PreOrderConfirmedEvent(Id, UserId));
    }

    public void MarkAsReadyToShip()
    {
        if (Status != PreOrderStatus.Confirmed)
            throw new DomainException("Sadece onaylanmış ön siparişler sevkiyata hazır olarak işaretlenebilir");

        Status = PreOrderStatus.ReadyToShip;
        ActualAvailabilityDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PreOrderReadyToShipEvent(Id, UserId, ActualAvailabilityDate.Value));
    }

    public void ConvertToOrder(Guid orderId)
    {
        if (Status != PreOrderStatus.ReadyToShip)
            throw new DomainException("Sadece sevkiyata hazır ön siparişler siparişe dönüştürülebilir");

        Status = PreOrderStatus.Converted;
        OrderId = orderId;
        ConvertedToOrderAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PreOrderConvertedEvent(Id, orderId, UserId));
    }

    public void Cancel()
    {
        if (Status == PreOrderStatus.Converted)
            throw new DomainException("Dönüştürülmüş ön siparişler iptal edilemez");
        if (Status == PreOrderStatus.Cancelled)
            throw new DomainException("Zaten iptal edilmiş ön sipariş");

        Status = PreOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PreOrderCancelledEvent(Id, UserId));
    }

    public void MarkAsExpired()
    {
        if (Status == PreOrderStatus.Converted || Status == PreOrderStatus.Cancelled)
            return; // Already in final state

        if (DateTime.UtcNow < ExpiresAt)
            throw new DomainException("Ön sipariş henüz süresi dolmamış");

        Status = PreOrderStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new PreOrderExpiredEvent(Id, UserId));
    }

    public void MarkNotificationAsSent()
    {
        if (NotificationSentAt.HasValue)
            return; // Idempotent operation

        NotificationSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

