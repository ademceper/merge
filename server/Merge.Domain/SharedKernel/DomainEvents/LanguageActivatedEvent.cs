using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Language Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageActivatedEvent(
    Guid LanguageId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

