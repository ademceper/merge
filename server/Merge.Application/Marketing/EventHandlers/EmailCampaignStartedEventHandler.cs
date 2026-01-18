using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignStartedEventHandler(ILogger<EmailCampaignStartedEventHandler> logger) : INotificationHandler<EmailCampaignStartedEvent>
{
    public async Task Handle(EmailCampaignStartedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign started event received. CampaignId: {CampaignId}, Name: {Name}, TotalRecipients: {TotalRecipients}",
            notification.CampaignId, notification.Name, notification.TotalRecipients);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign started)
        // - Performance metrics
        // - Email sending queue initialization

        await Task.CompletedTask;
    }
}
