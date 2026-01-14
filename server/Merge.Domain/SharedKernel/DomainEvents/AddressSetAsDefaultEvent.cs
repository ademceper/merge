using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Address Set As Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AddressSetAsDefaultEvent(
    Guid AddressId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
