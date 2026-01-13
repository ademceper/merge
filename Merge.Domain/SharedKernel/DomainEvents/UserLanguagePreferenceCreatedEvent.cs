using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserLanguagePreference Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserLanguagePreferenceCreatedEvent(
    Guid UserLanguagePreferenceId,
    Guid UserId,
    Guid LanguageId,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
