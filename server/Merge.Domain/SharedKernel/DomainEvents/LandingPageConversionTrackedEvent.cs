using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LandingPageConversionTrackedEvent(
    Guid LandingPageId,
    int ConversionCount,
    decimal ConversionRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

