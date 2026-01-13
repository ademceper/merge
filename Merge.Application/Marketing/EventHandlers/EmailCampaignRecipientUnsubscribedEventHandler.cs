using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Unsubscribed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignRecipientUnsubscribedEventHandler(ILogger<EmailCampaignRecipientUnsubscribedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientUnsubscribedEvent>
{
    public async Task Handle(EmailCampaignRecipientUnsubscribedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "EmailCampaignRecipient unsubscribed event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Unsubscribe rate calculation
        // - External email service sync (unsubscribe)

        await Task.CompletedTask;
    }
}
