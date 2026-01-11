using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Review Rejected Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewRejectedEvent(
    Guid ReviewId,
    Guid UserId, // Review owner
    Guid ProductId,
    Guid RejectedByUserId, // Admin who rejected
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
