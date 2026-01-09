using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Language Set As Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageSetAsDefaultEvent(
    Guid LanguageId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

