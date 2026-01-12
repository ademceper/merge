using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FAQ Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FaqCreatedEvent(
    Guid FaqId,
    string Question,
    string Category) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
