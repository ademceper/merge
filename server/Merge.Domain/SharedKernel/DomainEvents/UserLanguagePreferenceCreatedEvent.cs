using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserLanguagePreferenceCreatedEvent(
    Guid UserLanguagePreferenceId,
    Guid UserId,
    Guid LanguageId,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
