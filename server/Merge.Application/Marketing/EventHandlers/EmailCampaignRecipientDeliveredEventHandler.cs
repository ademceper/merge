using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientDeliveredEventHandler(ILogger<EmailCampaignRecipientDeliveredEventHandler> logger) : INotificationHandler<EmailCampaignRecipientDeliveredEvent>
{
    public async Task Handle(EmailCampaignRecipientDeliveredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient delivered event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Delivery rate calculation

        await Task.CompletedTask;
    }
}
