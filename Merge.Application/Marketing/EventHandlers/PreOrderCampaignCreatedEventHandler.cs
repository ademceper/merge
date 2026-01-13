using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// PreOrderCampaign Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class PreOrderCampaignCreatedEventHandler(ILogger<PreOrderCampaignCreatedEventHandler> logger) : INotificationHandler<PreOrderCampaignCreatedEvent>
{
    public async Task Handle(PreOrderCampaignCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
