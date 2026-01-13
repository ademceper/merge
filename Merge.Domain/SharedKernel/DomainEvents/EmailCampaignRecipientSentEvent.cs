using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailCampaignRecipient Sent Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignRecipientSentEvent(
    Guid RecipientId,
    Guid CampaignId,
    Guid SubscriberId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
