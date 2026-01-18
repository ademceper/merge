using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignScheduledEventHandler(ILogger<EmailCampaignScheduledEventHandler> logger) : INotificationHandler<EmailCampaignScheduledEvent>
{
    public async Task Handle(EmailCampaignScheduledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign scheduled event received. CampaignId: {CampaignId}, Name: {Name}, ScheduledAt: {ScheduledAt}",
            notification.CampaignId, notification.Name, notification.ScheduledAt);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign scheduled)
        // - Queue job scheduling (background worker)
        // - Calendar integration

        await Task.CompletedTask;
    }
}
