using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserCurrencyPreferenceCreatedEvent(
    Guid UserCurrencyPreferenceId,
    Guid UserId,
    Guid CurrencyId,
    string CurrencyCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
