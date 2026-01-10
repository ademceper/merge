using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignCreatedEventHandler : INotificationHandler<EmailCampaignCreatedEvent>
{
    private readonly ILogger<EmailCampaignCreatedEventHandler> _logger;

    public EmailCampaignCreatedEventHandler(ILogger<EmailCampaignCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email campaign created event received. CampaignId: {CampaignId}, Name: {Name}, Type: {Type}",
            notification.CampaignId, notification.Name, notification.Type);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign created)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
