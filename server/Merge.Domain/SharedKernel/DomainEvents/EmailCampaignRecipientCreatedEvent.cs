using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailCampaignRecipient Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignRecipientCreatedEvent(
    Guid RecipientId,
    Guid CampaignId,
    Guid SubscriberId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
