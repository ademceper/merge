using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Bounced Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientBouncedEventHandler : INotificationHandler<EmailCampaignRecipientBouncedEvent>
{
    private readonly ILogger<EmailCampaignRecipientBouncedEventHandler> _logger;

    public EmailCampaignRecipientBouncedEventHandler(ILogger<EmailCampaignRecipientBouncedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientBouncedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogWarning(
            "EmailCampaignRecipient bounced event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ErrorMessage: {ErrorMessage}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId, notification.ErrorMessage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Bounce rate calculation
        // - Email address validation/cleanup
        // - Subscriber status update (mark as invalid email)

        await Task.CompletedTask;
    }
}
