using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record EmailCampaignRecipientFailedEvent(
    Guid RecipientId,
    Guid CampaignId,
    Guid SubscriberId,
    string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
