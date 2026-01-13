using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ProductTranslation Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ProductTranslationUpdatedEvent(
    Guid ProductTranslationId,
    Guid ProductId,
    string LanguageCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
