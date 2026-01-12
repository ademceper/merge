using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Return Request Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReturnRequestCreatedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, decimal RefundAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
