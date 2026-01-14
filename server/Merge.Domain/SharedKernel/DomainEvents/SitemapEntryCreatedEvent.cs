using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Sitemap Entry Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SitemapEntryCreatedEvent(
    Guid EntryId,
    string Url,
    string PageType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

