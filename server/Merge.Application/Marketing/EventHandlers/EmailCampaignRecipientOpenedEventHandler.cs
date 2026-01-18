using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientOpenedEventHandler(ILogger<EmailCampaignRecipientOpenedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientOpenedEvent>
{
    public async Task Handle(EmailCampaignRecipientOpenedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient opened event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Open rate calculation
        // - Engagement scoring

        await Task.CompletedTask;
    }
}
