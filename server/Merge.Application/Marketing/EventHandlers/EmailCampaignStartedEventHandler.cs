using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Started Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignStartedEventHandler(ILogger<EmailCampaignStartedEventHandler> logger) : INotificationHandler<EmailCampaignStartedEvent>
{
    public async Task Handle(EmailCampaignStartedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
