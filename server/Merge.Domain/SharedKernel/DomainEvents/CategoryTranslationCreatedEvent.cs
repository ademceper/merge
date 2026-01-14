using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CategoryTranslation Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CategoryTranslationCreatedEvent(
    Guid CategoryTranslationId,
    Guid CategoryId,
    Guid LanguageId,
    string LanguageCode,
    string CategoryName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
