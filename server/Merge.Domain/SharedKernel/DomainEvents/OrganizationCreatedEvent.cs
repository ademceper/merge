using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Organization Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrganizationCreatedEvent(
    Guid OrganizationId,
    string Name,
    string? Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
