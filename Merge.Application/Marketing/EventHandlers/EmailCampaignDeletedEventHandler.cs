using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailCampaign Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailCampaignDeletedEventHandler : INotificationHandler<EmailCampaignDeletedEvent>
{
    private readonly ILogger<EmailCampaignDeletedEventHandler> _logger;

    public EmailCampaignDeletedEventHandler(ILogger<EmailCampaignDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailCampaignDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailCampaign deleted event received. CampaignId: {CampaignId}, Name: {Name}",
            notification.CampaignId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
