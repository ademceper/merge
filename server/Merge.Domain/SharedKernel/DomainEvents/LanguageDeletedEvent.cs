using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Language Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageDeletedEvent(
    Guid LanguageId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

