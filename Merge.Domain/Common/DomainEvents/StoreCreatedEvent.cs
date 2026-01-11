using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

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
