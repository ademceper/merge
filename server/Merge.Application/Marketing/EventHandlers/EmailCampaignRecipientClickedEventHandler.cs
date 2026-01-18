using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientClickedEventHandler(ILogger<EmailCampaignRecipientClickedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientClickedEvent>
{
    public async Task Handle(EmailCampaignRecipientClickedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient clicked event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Click rate calculation
        // - Conversion tracking

        await Task.CompletedTask;
    }
}
