using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SitemapEntryRestoredEvent(
    Guid Id,
    string Url) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
