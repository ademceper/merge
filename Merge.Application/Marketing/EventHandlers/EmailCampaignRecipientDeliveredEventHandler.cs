using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Delivered Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientDeliveredEventHandler : INotificationHandler<EmailCampaignRecipientDeliveredEvent>
{
    private readonly ILogger<EmailCampaignRecipientDeliveredEventHandler> _logger;

    public EmailCampaignRecipientDeliveredEventHandler(ILogger<EmailCampaignRecipientDeliveredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientDeliveredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailCampaignRecipient delivered event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Delivery rate calculation

        await Task.CompletedTask;
    }
}
