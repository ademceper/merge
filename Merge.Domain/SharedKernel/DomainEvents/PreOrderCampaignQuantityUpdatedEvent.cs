using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PreOrderCampaign Quantity Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PreOrderCampaignQuantityUpdatedEvent(
    Guid CampaignId,
    int CurrentQuantity,
    int MaxQuantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
