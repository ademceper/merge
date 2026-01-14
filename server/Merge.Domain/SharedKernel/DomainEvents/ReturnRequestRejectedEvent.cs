using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Return Request Rejected Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReturnRequestRejectedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, string? RejectionReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
