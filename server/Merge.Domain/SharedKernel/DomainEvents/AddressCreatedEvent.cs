using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Address Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AddressCreatedEvent(
    Guid AddressId,
    Guid UserId,
    string City,
    string Country,
    bool IsDefault) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
