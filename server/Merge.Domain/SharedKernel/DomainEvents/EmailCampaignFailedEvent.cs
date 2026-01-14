using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailCampaign Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailCampaignFailedEvent(
    Guid CampaignId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
