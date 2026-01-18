using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerCommission Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerCommission : BaseEntity, IAggregateRoot
{
    public Guid SellerId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public decimal OrderAmount { get; private set; }
    public decimal CommissionRate { get; private set; } // Percentage
    public decimal CommissionAmount { get; private set; }
    public decimal PlatformFee { get; private set; } = 0;
    public decimal NetAmount { get; private set; } // CommissionAmount - PlatformFee
    public CommissionStatus Status { get; private set; } = CommissionStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? PaymentReference { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User Seller { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    public OrderItem OrderItem { get; private set; } = null!;

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private SellerCommission() { }

    public static SellerCommission Create(
        Guid sellerId,
        Guid orderId,
        Guid orderItemId,
        decimal orderAmount,
        decimal commissionRate,
        decimal platformFee = 0)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstDefault(orderItemId, nameof(orderItemId));
        Guard.AgainstNegativeOrZero(orderAmount, nameof(orderAmount));
        Guard.AgainstNegative(commissionRate, nameof(commissionRate));
        Guard.AgainstNegative(platformFee, nameof(platformFee));

        var commissionAmount = orderAmount * (commissionRate / 100);
        var netAmount = commissionAmount - platformFee;

        if (netAmount < 0)
            throw new DomainException("Net komisyon tutarı negatif olamaz");

        var commission = new SellerCommission
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            OrderAmount = orderAmount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            PlatformFee = platformFee,
            NetAmount = netAmount,
            Status = CommissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        commission.AddDomainEvent(new SellerCommissionCreatedEvent(
            commission.Id, sellerId, orderId, commissionAmount, netAmount));

        return commission;
    }

    public void Approve()
    {
        if (Status == CommissionStatus.Approved)
            throw new DomainException("Komisyon zaten onaylanmış");

        if (Status == CommissionStatus.Paid)
            throw new DomainException("Ödenmiş komisyon onaylanamaz");

        if (Status == CommissionStatus.Cancelled)
            throw new DomainException("İptal edilmiş komisyon onaylanamaz");

        Status = CommissionStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerCommissionApprovedEvent(Id, SellerId, NetAmount));
    }

    public void MarkAsPaid(string paymentReference)
    {
        Guard.AgainstNullOrEmpty(paymentReference, nameof(paymentReference));

        if (Status != CommissionStatus.Approved)
            throw new DomainException("Sadece onaylanmış komisyonlar ödenebilir");

        if (Status == CommissionStatus.Paid)
            throw new DomainException("Komisyon zaten ödenmiş");

        Status = CommissionStatus.Paid;
        PaidAt = DateTime.UtcNow;
        PaymentReference = paymentReference;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerCommissionPaidEvent(Id, SellerId, NetAmount, paymentReference));
    }

    public void Cancel()
    {
        if (Status == CommissionStatus.Paid)
            throw new DomainException("Ödenmiş komisyon iptal edilemez");

        if (Status == CommissionStatus.Cancelled)
            throw new DomainException("Komisyon zaten iptal edilmiş");

        Status = CommissionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerCommissionCancelledEvent(Id, SellerId, NetAmount));
    }

    public void RevertToApproved()
    {
        if (Status != CommissionStatus.Paid)
            throw new DomainException("Sadece ödenmiş komisyonlar onay durumuna geri döndürülebilir");

        Status = CommissionStatus.Approved;
        PaidAt = null;
        PaymentReference = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerCommissionRevertedEvent(Id, SellerId, NetAmount));
    }
}
