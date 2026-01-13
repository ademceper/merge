using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Address Removed Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AddressRemovedDefaultEvent(
    Guid AddressId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
