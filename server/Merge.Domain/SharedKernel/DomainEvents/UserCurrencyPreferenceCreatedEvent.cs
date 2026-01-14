using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserCurrencyPreference Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserCurrencyPreferenceCreatedEvent(
    Guid UserCurrencyPreferenceId,
    Guid UserId,
    Guid CurrencyId,
    string CurrencyCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
