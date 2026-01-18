using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class PreOrderCampaignCreatedEventHandler(ILogger<PreOrderCampaignCreatedEventHandler> logger) : INotificationHandler<PreOrderCampaignCreatedEvent>
{
    public async Task Handle(PreOrderCampaignCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PreOrderCampaign created event received. CampaignId: {CampaignId}, Name: {Name}, ProductId: {ProductId}",
            notification.CampaignId, notification.Name, notification.ProductId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (campaign created)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
