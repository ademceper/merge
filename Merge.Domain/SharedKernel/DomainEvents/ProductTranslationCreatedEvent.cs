using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ProductTranslation Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ProductTranslationCreatedEvent(
    Guid ProductTranslationId,
    Guid ProductId,
    Guid LanguageId,
    string LanguageCode,
    string ProductName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
