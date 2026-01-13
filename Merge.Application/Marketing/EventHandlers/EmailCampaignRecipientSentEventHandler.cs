using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Sent Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientSentEventHandler : INotificationHandler<EmailCampaignRecipientSentEvent>
{
    private readonly ILogger<EmailCampaignRecipientSentEventHandler> _logger;

    public EmailCampaignRecipientSentEventHandler(ILogger<EmailCampaignRecipientSentEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailCampaignRecipient sent event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Email delivery tracking

        await Task.CompletedTask;
    }
}
