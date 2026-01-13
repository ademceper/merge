using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// AbandonedCartEmail Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class AbandonedCartEmail : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CartId { get; private set; }
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public AbandonedCartEmailType EmailType { get; private set; }
    
    public DateTime SentAt { get; private set; }
    public bool WasOpened { get; private set; }
    public bool WasClicked { get; private set; }
    public bool ResultedInPurchase { get; private set; }
    public Guid? CouponId { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Coupon? Coupon { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private AbandonedCartEmail() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static AbandonedCartEmail Create(
        Guid cartId,
        Guid userId,
        AbandonedCartEmailType emailType,
        Guid? couponId = null)
    {
        Guard.AgainstDefault(cartId, nameof(cartId));
        Guard.AgainstDefault(userId, nameof(userId));

        var abandonedCartEmail = new AbandonedCartEmail
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            UserId = userId,
            EmailType = emailType,
            SentAt = DateTime.UtcNow,
            WasOpened = false,
            WasClicked = false,
            ResultedInPurchase = false,
            CouponId = couponId,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - AbandonedCartEmailCreatedEvent
        abandonedCartEmail.AddDomainEvent(new AbandonedCartEmailCreatedEvent(abandonedCartEmail.Id, cartId, userId, emailType));

        return abandonedCartEmail;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as opened
    public void MarkAsOpened()
    {
        if (WasOpened)
            return; // Idempotent operation

        WasOpened = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - AbandonedCartEmailOpenedEvent
        AddDomainEvent(new AbandonedCartEmailOpenedEvent(Id, CartId, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as clicked
    public void MarkAsClicked()
    {
        if (WasClicked)
            return; // Idempotent operation

        WasClicked = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - AbandonedCartEmailClickedEvent
        AddDomainEvent(new AbandonedCartEmailClickedEvent(Id, CartId, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as resulted in purchase
    public void MarkAsResultedInPurchase()
    {
        if (ResultedInPurchase)
            return; // Idempotent operation

        ResultedInPurchase = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - AbandonedCartEmailResultedInPurchaseEvent
        AddDomainEvent(new AbandonedCartEmailResultedInPurchaseEvent(Id, CartId, UserId));
    }
}
