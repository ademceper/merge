using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Purchase Order Cancelled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PurchaseOrderCancelledEvent(Guid PurchaseOrderId, Guid OrganizationId, string PONumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

