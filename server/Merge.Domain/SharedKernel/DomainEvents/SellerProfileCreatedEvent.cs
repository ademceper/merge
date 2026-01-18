using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerProfileCreatedEvent(
    Guid ProfileId,
    Guid UserId,
    string StoreName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
