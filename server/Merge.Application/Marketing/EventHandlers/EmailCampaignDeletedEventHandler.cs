using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignDeletedEventHandler(ILogger<EmailCampaignDeletedEventHandler> logger) : INotificationHandler<EmailCampaignDeletedEvent>
{
    public async Task Handle(EmailCampaignDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaign deleted event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
