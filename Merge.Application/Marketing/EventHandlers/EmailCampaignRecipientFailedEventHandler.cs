using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Failed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailCampaignRecipientFailedEventHandler(ILogger<EmailCampaignRecipientFailedEventHandler> logger) : INotificationHandler<EmailCampaignRecipientFailedEvent>
{
    public async Task Handle(EmailCampaignRecipientFailedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogError(
            "EmailCampaignRecipient failed event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ErrorMessage: {ErrorMessage}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId, notification.ErrorMessage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Failure rate calculation
        // - Retry mechanism
        // - Alert notification

        await Task.CompletedTask;
    }
}
