using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SEOSettings Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SEOSettingsRestoredEvent(
    Guid Id,
    string PageType,
    Guid? EntityId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
