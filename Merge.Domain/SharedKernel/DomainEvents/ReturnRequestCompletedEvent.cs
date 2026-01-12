using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Return Request Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReturnRequestCompletedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, string? TrackingNumber, DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
