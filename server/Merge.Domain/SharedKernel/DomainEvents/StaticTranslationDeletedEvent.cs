using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// StaticTranslation Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record StaticTranslationDeletedEvent(
    Guid TranslationId,
    string Key,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
