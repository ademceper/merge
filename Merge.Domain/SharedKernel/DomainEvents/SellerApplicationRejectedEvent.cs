using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerApplication Rejected Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerApplicationRejectedEvent(
    Guid ApplicationId,
    Guid UserId,
    Guid RejectedBy,
    string RejectionReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
