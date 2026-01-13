using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Sent Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignSentEventHandler(ILogger<EmailCampaignSentEventHandler> logger) : INotificationHandler<EmailCampaignSentEvent>
{
    public async Task Handle(EmailCampaignSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email campaign sent event received. CampaignId: {CampaignId}, SentCount: {SentCount}",
            notification.CampaignId, notification.SentCount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Performance metrics
        // - Notification gönderimi (campaign completed)

        await Task.CompletedTask;
    }
}
