using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record StoreCreatedEvent(
    Guid StoreId,
    Guid SellerId,
    string StoreName,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
