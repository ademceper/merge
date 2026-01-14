using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Scheduled Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignScheduledEventHandler(ILogger<EmailCampaignScheduledEventHandler> logger) : INotificationHandler<EmailCampaignScheduledEvent>
{
    public async Task Handle(EmailCampaignScheduledEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email campaign scheduled event received. CampaignId: {CampaignId}, Name: {Name}, ScheduledAt: {ScheduledAt}",
            notification.CampaignId, notification.Name, notification.ScheduledAt);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign scheduled)
        // - Queue job scheduling (background worker)
        // - Calendar integration

        await Task.CompletedTask;
    }
}
