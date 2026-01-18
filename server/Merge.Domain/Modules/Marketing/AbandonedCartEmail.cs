using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;


public class AbandonedCartEmail : BaseEntity, IAggregateRoot
{
    public Guid CartId { get; private set; }
    public Guid UserId { get; private set; }
    
    public AbandonedCartEmailType EmailType { get; private set; }
    
    public DateTime SentAt { get; private set; }
    public bool WasOpened { get; private set; }
    public bool WasClicked { get; private set; }
    public bool ResultedInPurchase { get; private set; }
    public Guid? CouponId { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Cart Cart { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Coupon? Coupon { get; private set; }

    private AbandonedCartEmail() { }

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

        abandonedCartEmail.AddDomainEvent(new AbandonedCartEmailCreatedEvent(abandonedCartEmail.Id, cartId, userId, emailType));

        return abandonedCartEmail;
    }

    public void MarkAsOpened()
    {
        if (WasOpened)
            return; // Idempotent operation

        WasOpened = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AbandonedCartEmailOpenedEvent(Id, CartId, UserId));
    }

    public void MarkAsClicked()
    {
        if (WasClicked)
            return; // Idempotent operation

        WasClicked = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AbandonedCartEmailClickedEvent(Id, CartId, UserId));
    }

    public void MarkAsResultedInPurchase()
    {
        if (ResultedInPurchase)
            return; // Idempotent operation

        ResultedInPurchase = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AbandonedCartEmailResultedInPurchaseEvent(Id, CartId, UserId));
    }
}
