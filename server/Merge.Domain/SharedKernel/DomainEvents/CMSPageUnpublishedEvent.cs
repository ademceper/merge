using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CMS Page Unpublished Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record CMSPageUnpublishedEvent(
    Guid PageId,
    string Title,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
