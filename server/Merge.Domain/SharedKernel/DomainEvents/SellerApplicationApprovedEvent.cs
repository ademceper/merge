using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerApplication Approved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerApplicationApprovedEvent(
    Guid ApplicationId,
    Guid UserId,
    Guid ApprovedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
