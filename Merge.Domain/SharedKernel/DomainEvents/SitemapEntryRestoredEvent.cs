using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SitemapEntry Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SitemapEntryRestoredEvent(
    Guid Id,
    string Url) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
