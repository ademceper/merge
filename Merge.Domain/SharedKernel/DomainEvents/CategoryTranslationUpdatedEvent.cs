using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CategoryTranslation Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CategoryTranslationUpdatedEvent(
    Guid CategoryTranslationId,
    Guid CategoryId,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
