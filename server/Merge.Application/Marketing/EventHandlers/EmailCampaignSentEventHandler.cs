using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignSentEventHandler(ILogger<EmailCampaignSentEventHandler> logger) : INotificationHandler<EmailCampaignSentEvent>
{
    public async Task Handle(EmailCampaignSentEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign sent event received. CampaignId: {CampaignId}, SentCount: {SentCount}",
            notification.CampaignId, notification.SentCount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Performance metrics
        // - Notification gönderimi (campaign completed)

        await Task.CompletedTask;
    }
}
