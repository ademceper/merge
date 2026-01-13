using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Clicked Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientClickedEventHandler : INotificationHandler<EmailCampaignRecipientClickedEvent>
{
    private readonly ILogger<EmailCampaignRecipientClickedEventHandler> _logger;

    public EmailCampaignRecipientClickedEventHandler(ILogger<EmailCampaignRecipientClickedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientClickedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailCampaignRecipient clicked event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Click rate calculation
        // - Conversion tracking

        await Task.CompletedTask;
    }
}
