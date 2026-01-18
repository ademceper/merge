using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientBouncedEventHandler(ILogger<EmailCampaignRecipientBouncedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientBouncedEvent>
{
    public async Task Handle(EmailCampaignRecipientBouncedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "EmailCampaignRecipient bounced event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ErrorMessage: {ErrorMessage}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId, notification.ErrorMessage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Bounce rate calculation
        // - Email address validation/cleanup
        // - Subscriber status update (mark as invalid email)

        await Task.CompletedTask;
    }
}
