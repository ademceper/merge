using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SEO Settings Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record SEOSettingsUpdatedEvent(
    Guid SettingsId,
    string PageType,
    Guid? EntityId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

