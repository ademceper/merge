using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignUpdatedEventHandler(ILogger<EmailCampaignUpdatedEventHandler> logger) : INotificationHandler<EmailCampaignUpdatedEvent>
{
    public async Task Handle(EmailCampaignUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign updated event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Notification gönderimi (campaign updated)
        // - External system sync

        await Task.CompletedTask;
    }
}
