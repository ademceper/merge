using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReturnRequestRejectedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, string? RejectionReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
