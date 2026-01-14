using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Currency Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CurrencyDeletedEvent(
    Guid CurrencyId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

