using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientFailedEventHandler(ILogger<EmailCampaignRecipientFailedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientFailedEvent>
{
    public async Task Handle(EmailCampaignRecipientFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogError(
            "EmailCampaignRecipient failed event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ErrorMessage: {ErrorMessage}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId, notification.ErrorMessage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Failure rate calculation
        // - Retry mechanism
        // - Alert notification

        await Task.CompletedTask;
    }
}
