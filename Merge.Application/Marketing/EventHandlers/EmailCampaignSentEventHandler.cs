using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Campaign Sent Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignSentEventHandler : INotificationHandler<EmailCampaignSentEvent>
{
    private readonly ILogger<EmailCampaignSentEventHandler> _logger;

    public EmailCampaignSentEventHandler(ILogger<EmailCampaignSentEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignSentEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email campaign sent event received. CampaignId: {CampaignId}, SentCount: {SentCount}",
            notification.CampaignId, notification.SentCount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Performance metrics
        // - Notification gönderimi (campaign completed)

        await Task.CompletedTask;
    }
}
