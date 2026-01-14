using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CMS Page Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record CMSPageDeletedEvent(
    Guid PageId,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

