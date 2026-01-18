using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReturnRequestApprovedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, DateTime ApprovedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
