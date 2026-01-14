using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// StaticTranslation Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record StaticTranslationRestoredEvent(
    Guid Id,
    string Key,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
