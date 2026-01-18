using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientUnsubscribedEventHandler(ILogger<EmailCampaignRecipientUnsubscribedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientUnsubscribedEvent>
{
    public async Task Handle(EmailCampaignRecipientUnsubscribedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient unsubscribed event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Unsubscribe rate calculation
        // - External email service sync (unsubscribe)

        await Task.CompletedTask;
    }
}
