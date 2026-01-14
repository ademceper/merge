using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Failed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignFailedEventHandler(ILogger<EmailCampaignFailedEventHandler> logger) : INotificationHandler<EmailCampaignFailedEvent>
{
    public async Task Handle(EmailCampaignFailedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogError(
            "Email campaign failed event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Alert to administrators (critical)
        // - Error reporting
        // - Retry mechanism
        // - Notification gönderimi (campaign failed)

        await Task.CompletedTask;
    }
}
