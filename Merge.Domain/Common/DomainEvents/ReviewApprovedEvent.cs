using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Review Approved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewApprovedEvent(
    Guid ReviewId,
    Guid UserId, // Review owner
    Guid ProductId,
    int Rating,
    Guid ApprovedByUserId) : IDomainEvent // Admin who approved
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
