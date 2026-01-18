using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CategoryTranslationCreatedEvent(
    Guid CategoryTranslationId,
    Guid CategoryId,
    Guid LanguageId,
    string LanguageCode,
    string CategoryName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
