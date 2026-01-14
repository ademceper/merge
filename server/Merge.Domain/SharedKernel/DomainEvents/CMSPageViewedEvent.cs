using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CMSPage Viewed Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record CMSPageViewedEvent(
    Guid PageId,
    string Title,
    int ViewCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
