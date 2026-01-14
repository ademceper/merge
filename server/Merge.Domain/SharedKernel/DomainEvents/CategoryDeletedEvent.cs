using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Category Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record CategoryDeletedEvent(
    Guid CategoryId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

