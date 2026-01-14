using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Organization Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrganizationDeletedEvent(
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
