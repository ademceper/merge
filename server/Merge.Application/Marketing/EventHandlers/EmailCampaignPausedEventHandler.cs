using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignPausedEventHandler(ILogger<EmailCampaignPausedEventHandler> logger) : INotificationHandler<EmailCampaignPausedEvent>
{
    public async Task Handle(EmailCampaignPausedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign paused event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign paused)
        // - Email sending queue pause
        // - Alert to administrators

        await Task.CompletedTask;
    }
}
