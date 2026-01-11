using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerCommission Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerCommission : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
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

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User Seller { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    public OrderItem OrderItem { get; private set; } = null!;

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SellerCommission() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Event - SellerCommission Created
        commission.AddDomainEvent(new SellerCommissionCreatedEvent(
            commission.Id, sellerId, orderId, commissionAmount, netAmount));

        return commission;
    }

    // ✅ BOLUM 1.1: Domain Method - Approve commission
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

        // ✅ BOLUM 1.5: Domain Event - SellerCommission Approved
        AddDomainEvent(new SellerCommissionApprovedEvent(Id, SellerId, NetAmount));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as paid
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

        // ✅ BOLUM 1.5: Domain Event - SellerCommission Paid
        AddDomainEvent(new SellerCommissionPaidEvent(Id, SellerId, NetAmount, paymentReference));
    }

    // ✅ BOLUM 1.1: Domain Method - Cancel commission
    public void Cancel()
    {
        if (Status == CommissionStatus.Paid)
            throw new DomainException("Ödenmiş komisyon iptal edilemez");

        if (Status == CommissionStatus.Cancelled)
            throw new DomainException("Komisyon zaten iptal edilmiş");

        Status = CommissionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerCommission Cancelled
        AddDomainEvent(new SellerCommissionCancelledEvent(Id, SellerId, NetAmount));
    }

    // ✅ BOLUM 1.1: Domain Method - Revert paid commission back to approved (for failed payouts)
    public void RevertToApproved()
    {
        if (Status != CommissionStatus.Paid)
            throw new DomainException("Sadece ödenmiş komisyonlar onay durumuna geri döndürülebilir");

        Status = CommissionStatus.Approved;
        PaidAt = null;
        PaymentReference = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerCommission Reverted
        AddDomainEvent(new SellerCommissionRevertedEvent(Id, SellerId, NetAmount));
    }
}
