using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// EmailCampaign Started Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignStartedEvent(
    Guid CampaignId,
    string Name,
    int TotalRecipients) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
