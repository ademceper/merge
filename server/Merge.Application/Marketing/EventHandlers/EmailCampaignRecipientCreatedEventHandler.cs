using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignRecipientCreatedEventHandler(ILogger<EmailCampaignRecipientCreatedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientCreatedEvent>
{
    public async Task Handle(EmailCampaignRecipientCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailCampaignRecipient created event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
