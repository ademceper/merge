using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Team Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TeamUpdatedEvent(
    Guid TeamId,
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
