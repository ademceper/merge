using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Language Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageUpdatedEvent(
    Guid LanguageId,
    string Code,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

