using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Sent Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignRecipientSentEventHandler(ILogger<EmailCampaignRecipientSentEventHandler> logger) : INotificationHandler<EmailCampaignRecipientSentEvent>
{
    public async Task Handle(EmailCampaignRecipientSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "EmailCampaignRecipient sent event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Email delivery tracking

        await Task.CompletedTask;
    }
}
