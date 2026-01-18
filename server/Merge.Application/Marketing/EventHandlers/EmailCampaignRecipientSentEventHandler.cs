using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientSentEventHandler(ILogger<EmailCampaignRecipientSentEventHandler> logger) : INotificationHandler<EmailCampaignRecipientSentEvent>
{
    public async Task Handle(EmailCampaignRecipientSentEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient sent event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Email delivery tracking

        await Task.CompletedTask;
    }
}
