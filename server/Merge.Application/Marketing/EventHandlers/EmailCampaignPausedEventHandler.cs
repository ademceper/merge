using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Paused Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignPausedEventHandler(ILogger<EmailCampaignPausedEventHandler> logger) : INotificationHandler<EmailCampaignPausedEvent>
{
    public async Task Handle(EmailCampaignPausedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email campaign paused event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign paused)
        // - Email sending queue pause
        // - Alert to administrators

        await Task.CompletedTask;
    }
}
