using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LoyaltyTierCreatedEvent(
    Guid TierId,
    string Name,
    int Level,
    int MinimumPoints) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
