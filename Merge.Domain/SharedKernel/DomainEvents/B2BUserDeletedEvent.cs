using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// B2B User Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record B2BUserDeletedEvent(
    Guid B2BUserId,
    Guid UserId,
    Guid OrganizationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
