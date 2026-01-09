using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Sitemap Entry Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SitemapEntryUpdatedEvent(
    Guid EntryId,
    string Url) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

