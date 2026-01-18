using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailCampaignCancelledEventHandler(ILogger<EmailCampaignCancelledEventHandler> logger) : INotificationHandler<EmailCampaignCancelledEvent>
{
    public async Task Handle(EmailCampaignCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email campaign cancelled event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign cancelled)
        // - Email sending queue cancellation
        // - Resource cleanup

        await Task.CompletedTask;
    }
}
