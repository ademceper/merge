using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserLanguagePreference Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserLanguagePreferenceUpdatedEvent(
    Guid UserLanguagePreferenceId,
    Guid UserId,
    Guid LanguageId,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
