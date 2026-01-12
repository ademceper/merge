using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Points Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PointsAddedEvent(
    Guid AccountId,
    Guid UserId,
    int Points,
    int NewBalance,
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
