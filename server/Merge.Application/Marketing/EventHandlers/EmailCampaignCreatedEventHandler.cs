using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignCreatedEventHandler(ILogger<EmailCampaignCreatedEventHandler> logger) : INotificationHandler<EmailCampaignCreatedEvent>
{
    public async Task Handle(EmailCampaignCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
