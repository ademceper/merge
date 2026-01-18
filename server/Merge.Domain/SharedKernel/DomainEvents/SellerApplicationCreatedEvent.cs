using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerApplicationCreatedEvent(
    Guid ApplicationId,
    Guid UserId,
    string BusinessName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
