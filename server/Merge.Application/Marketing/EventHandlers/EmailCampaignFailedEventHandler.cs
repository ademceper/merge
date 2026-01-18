using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignFailedEventHandler(ILogger<EmailCampaignFailedEventHandler> logger) : INotificationHandler<EmailCampaignFailedEvent>
{
    public async Task Handle(EmailCampaignFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogError(
            "Email campaign failed event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Alert to administrators (critical)
        // - Error reporting
        // - Retry mechanism
        // - Notification gönderimi (campaign failed)

        await Task.CompletedTask;
    }
}
