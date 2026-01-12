using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// ReturnRequest Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ReturnRequest : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string Reason { get; private set; } = string.Empty; // Defective, WrongItem, NotAsDescribed, ChangedMind
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public ReturnRequestStatus Status { get; private set; } = ReturnRequestStatus.Pending;
    public string? RejectionReason { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string? TrackingNumber { get; private set; } // İade kargo takip no
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public List<Guid> OrderItemIds { get; private set; } = new List<Guid>(); // İade edilecek kalemler

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ReturnRequest() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static ReturnRequest Create(
        Guid orderId,
        Guid userId,
        string reason,
        decimal refundAmount,
        List<Guid> orderItemIds,
        Order order,
        User user)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(reason, nameof(reason));
        Guard.AgainstNegative(refundAmount, nameof(refundAmount));
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
            RefundAmount = refundAmount,
            OrderItemIds = orderItemIds,
            Status = ReturnRequestStatus.Pending,
            Order = order,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - Return Request Created
        returnRequest.AddDomainEvent(new ReturnRequestCreatedEvent(
            returnRequest.Id,
            orderId,
            userId,
            refundAmount));

        return returnRequest;
    }

    // ✅ BOLUM 1.1: Domain Logic - Approve return request
    public void Approve()
    {
        if (Status != ReturnRequestStatus.Pending)
            throw new DomainException("Sadece bekleyen iade talepleri onaylanabilir");

        Status = ReturnRequestStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Return Request Approved
        AddDomainEvent(new ReturnRequestApprovedEvent(Id, OrderId, UserId, ApprovedAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Reject return request
    public void Reject(string? rejectionReason = null)
    {
        if (Status != ReturnRequestStatus.Pending)
            throw new DomainException("Sadece bekleyen iade talepleri reddedilebilir");

        Status = ReturnRequestStatus.Rejected;
        RejectionReason = rejectionReason;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Return Request Rejected
        AddDomainEvent(new ReturnRequestRejectedEvent(Id, OrderId, UserId, rejectionReason));
    }

    // ✅ BOLUM 1.1: Domain Logic - Complete return request
    public void Complete(string? trackingNumber = null)
    {
        if (Status != ReturnRequestStatus.Approved && Status != ReturnRequestStatus.Processing)
            throw new DomainException("Sadece onaylanmış veya işlenmekte olan iade talepleri tamamlanabilir");

        Status = ReturnRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Return Request Completed
        AddDomainEvent(new ReturnRequestCompletedEvent(Id, OrderId, UserId, trackingNumber, CompletedAt.Value));
    }
}

