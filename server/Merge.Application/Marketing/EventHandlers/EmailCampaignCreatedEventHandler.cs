using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignCreatedEventHandler(ILogger<EmailCampaignCreatedEventHandler> logger) : INotificationHandler<EmailCampaignCreatedEvent>
{
    public async Task Handle(EmailCampaignCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign created event received. CampaignId: {CampaignId}, Name: {Name}, Type: {Type}",
            notification.CampaignId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign created)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
