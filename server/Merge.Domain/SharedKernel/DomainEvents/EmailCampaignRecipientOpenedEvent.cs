using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailCampaignRecipient Opened Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignRecipientOpenedEvent(
    Guid RecipientId,
    Guid CampaignId,
    Guid SubscriberId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
