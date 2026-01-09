using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Currency Set As Base Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CurrencySetAsBaseEvent(
    Guid CurrencyId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

