using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SitemapEntry Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SitemapEntryDeletedEvent(
    Guid EntryId,
    string Url) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
