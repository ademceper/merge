using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SitemapEntryUpdatedEvent(
    Guid EntryId,
    string Url) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

