using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PreOrderCampaignQuantityUpdatedEvent(
    Guid CampaignId,
    int CurrentQuantity,
    int MaxQuantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
