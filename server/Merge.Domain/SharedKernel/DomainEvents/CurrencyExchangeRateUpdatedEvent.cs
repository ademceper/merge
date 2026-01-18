using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CurrencyExchangeRateUpdatedEvent(
    Guid CurrencyId,
    string Code,
    decimal OldRate,
    decimal NewRate,
    string Source) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

