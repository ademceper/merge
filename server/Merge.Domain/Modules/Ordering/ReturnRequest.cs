using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// ReturnRequest Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReturnRequest : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string Reason { get; private set; } = string.Empty; // Defective, WrongItem, NotAsDescribed, ChangedMind
    public ReturnRequestStatus Status { get; private set; } = ReturnRequestStatus.Pending;
    public string? RejectionReason { get; private set; }
    
    private decimal _refundAmount;
    
    // Database column (EF Core mapping)
    public decimal RefundAmount 
    { 
        get => _refundAmount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(RefundAmount));
            _refundAmount = value;
        }
    }
    
    [NotMapped]
    public Money RefundAmountMoney => new Money(_refundAmount);
    
    public string? TrackingNumber { get; private set; } // İade kargo takip no
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public List<Guid> OrderItemIds { get; private set; } = []; // İade edilecek kalemler

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private ReturnRequest() { }

    public static ReturnRequest Create(
        Guid orderId,
        Guid userId,
        string reason,
        Money refundAmount,
        List<Guid> orderItemIds,
        Order order,
        User user)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(reason, nameof(reason));
        Guard.AgainstNull(refundAmount, nameof(refundAmount));
        Guard.AgainstNegative(refundAmount.Amount, nameof(refundAmount));
        Guard.AgainstNull(orderItemIds, nameof(orderItemIds));
        Guard.AgainstNull(order, nameof(order));
        Guard.AgainstNull(user, nameof(user));

        if (orderItemIds.Count == 0)
            throw new DomainException("İade edilecek ürün seçilmelidir");

        var returnRequest = new ReturnRequest
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Reason = reason,
            _refundAmount = refundAmount.Amount, // EF Core compatibility - backing field
            OrderItemIds = orderItemIds,
            Status = ReturnRequestStatus.Pending,
            Order = order,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        returnRequest.AddDomainEvent(new ReturnRequestCreatedEvent(
            returnRequest.Id,
            orderId,
            userId,
            refundAmount.Amount));

        return returnRequest;
    }

    public void Approve()
    {
        if (Status != ReturnRequestStatus.Pending)
            throw new DomainException("Sadece bekleyen iade talepleri onaylanabilir");

        Status = ReturnRequestStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReturnRequestApprovedEvent(Id, OrderId, UserId, ApprovedAt.Value));
    }

    public void Reject(string? rejectionReason = null)
    {
        if (Status != ReturnRequestStatus.Pending)
            throw new DomainException("Sadece bekleyen iade talepleri reddedilebilir");

        Status = ReturnRequestStatus.Rejected;
        RejectionReason = rejectionReason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReturnRequestRejectedEvent(Id, OrderId, UserId, rejectionReason));
    }

    public void Complete(string? trackingNumber = null)
    {
        if (Status != ReturnRequestStatus.Approved && Status != ReturnRequestStatus.Processing)
            throw new DomainException("Sadece onaylanmış veya işlenmekte olan iade talepleri tamamlanabilir");

        Status = ReturnRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReturnRequestCompletedEvent(Id, OrderId, UserId, trackingNumber, CompletedAt.Value));
    }
}

