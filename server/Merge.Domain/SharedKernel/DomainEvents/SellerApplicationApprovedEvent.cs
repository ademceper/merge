using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerApplicationApprovedEvent(
    Guid ApplicationId,
    Guid UserId,
    Guid ApprovedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
