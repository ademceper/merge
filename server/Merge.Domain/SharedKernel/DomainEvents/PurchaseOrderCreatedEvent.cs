using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Purchase Order Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PurchaseOrderCreatedEvent(Guid PurchaseOrderId, Guid OrganizationId, Guid? B2BUserId, string PONumber, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

