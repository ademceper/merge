using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Role Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record RoleCreatedEvent(
    Guid RoleId,
    string Name,
    string? Description) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
