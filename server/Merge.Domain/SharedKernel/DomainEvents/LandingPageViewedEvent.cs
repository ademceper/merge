using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LandingPage Viewed Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record LandingPageViewedEvent(
    Guid LandingPageId,
    string Name,
    int ViewCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
