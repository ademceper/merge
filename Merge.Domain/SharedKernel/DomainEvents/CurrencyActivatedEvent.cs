using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Currency Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CurrencyActivatedEvent(
    Guid CurrencyId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

