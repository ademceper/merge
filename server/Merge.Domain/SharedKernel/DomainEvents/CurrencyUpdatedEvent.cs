using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Currency Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CurrencyUpdatedEvent(
    Guid CurrencyId,
    string Code,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

