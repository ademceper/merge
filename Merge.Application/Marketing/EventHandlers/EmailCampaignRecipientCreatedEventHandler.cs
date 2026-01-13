using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaignRecipient Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientCreatedEventHandler : INotificationHandler<EmailCampaignRecipientCreatedEvent>
{
    private readonly ILogger<EmailCampaignRecipientCreatedEventHandler> _logger;

    public EmailCampaignRecipientCreatedEventHandler(ILogger<EmailCampaignRecipientCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignRecipientCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailCampaignRecipient created event received. RecipientId: {RecipientId}, CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            notification.RecipientId, notification.CampaignId, notification.SubscriberId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
