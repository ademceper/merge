using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Language Set As Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LanguageSetAsDefaultEvent(
    Guid LanguageId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

