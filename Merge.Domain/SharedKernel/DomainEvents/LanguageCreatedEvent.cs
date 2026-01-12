using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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

