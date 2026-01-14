using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Organization Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrganizationUpdatedEvent(
    Guid OrganizationId,
    string Name,
    IReadOnlyList<string> ChangedFields) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    // Backward compatibility için overload
    public OrganizationUpdatedEvent(Guid organizationId, string name)
        : this(organizationId, name, Array.Empty<string>())
    {
    }
}
