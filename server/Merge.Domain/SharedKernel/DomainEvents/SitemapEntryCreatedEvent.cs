using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SitemapEntryCreatedEvent(
    Guid EntryId,
    string Url,
    string PageType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

