using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CMSPage Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record CMSPageRestoredEvent(
    Guid Id,
    string Title,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
