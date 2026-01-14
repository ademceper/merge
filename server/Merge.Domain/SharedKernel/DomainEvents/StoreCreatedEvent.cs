using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Store Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreCreatedEvent(
    Guid StoreId,
    Guid SellerId,
    string StoreName,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
