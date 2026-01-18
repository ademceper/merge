using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReturnRequestCompletedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, string? TrackingNumber, DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
