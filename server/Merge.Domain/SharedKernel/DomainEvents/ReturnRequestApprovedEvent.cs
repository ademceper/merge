using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Return Request Approved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReturnRequestApprovedEvent(Guid ReturnRequestId, Guid OrderId, Guid UserId, DateTime ApprovedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
