using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Failed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientFailedEventHandler : INotificationHandler<EmailCampaignRecipientFailedEvent>
{
    private readonly ILogger<EmailCampaignRecipientFailedEventHandler> _logger;

    public EmailCampaignRecipientFailedEventHandler(ILogger<EmailCampaignRecipientFailedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientFailedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogError(
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
