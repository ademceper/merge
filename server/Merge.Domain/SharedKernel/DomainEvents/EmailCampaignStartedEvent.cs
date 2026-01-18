using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record EmailCampaignStartedEvent(
    Guid CampaignId,
    string Name,
    int TotalRecipients) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
