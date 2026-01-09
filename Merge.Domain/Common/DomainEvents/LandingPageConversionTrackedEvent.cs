using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Landing Page Conversion Tracked Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record LandingPageConversionTrackedEvent(
    Guid LandingPageId,
    int ConversionCount,
    decimal ConversionRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

