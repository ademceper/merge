using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PointsDeductedEvent(
    Guid AccountId,
    Guid UserId,
    int Points,
    int NewBalance,
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
