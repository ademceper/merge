using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ProductTranslationCreatedEvent(
    Guid ProductTranslationId,
    Guid ProductId,
    Guid LanguageId,
    string LanguageCode,
    string ProductName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
