using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LoyaltyTier Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LoyaltyTierCreatedEvent(
    Guid TierId,
    string Name,
    int Level,
    int MinimumPoints) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
