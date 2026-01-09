using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Language Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageCreatedEvent(
    Guid LanguageId,
    string Code,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

