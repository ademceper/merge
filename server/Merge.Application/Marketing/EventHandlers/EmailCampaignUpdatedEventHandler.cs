using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignUpdatedEventHandler(ILogger<EmailCampaignUpdatedEventHandler> logger) : INotificationHandler<EmailCampaignUpdatedEvent>
{
    public async Task Handle(EmailCampaignUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
