using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailCampaignRecipient Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignRecipientFailedEvent(
    Guid RecipientId,
    Guid CampaignId,
    Guid SubscriberId,
    string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
