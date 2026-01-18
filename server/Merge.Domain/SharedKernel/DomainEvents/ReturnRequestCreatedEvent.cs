using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReturnRequestCreatedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, decimal RefundAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
