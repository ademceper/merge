using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// StaticTranslation Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record StaticTranslationCreatedEvent(
    Guid TranslationId,
    string Key,
    string LanguageCode,
    string Category) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
