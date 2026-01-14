using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Purchase Order Submitted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PurchaseOrderSubmittedEvent(Guid PurchaseOrderId, Guid OrganizationId, string PONumber, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

