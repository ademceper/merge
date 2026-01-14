using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// B2B User Approved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record B2BUserApprovedEvent(Guid B2BUserId, Guid UserId, Guid OrganizationId, Guid ApprovedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

