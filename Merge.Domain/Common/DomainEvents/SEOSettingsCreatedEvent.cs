using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SEO Settings Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SEOSettingsCreatedEvent(
    Guid SettingsId,
    string PageType,
    Guid? EntityId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

