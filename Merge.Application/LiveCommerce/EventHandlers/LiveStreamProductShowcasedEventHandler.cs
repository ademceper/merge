using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Product Showcased Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamProductShowcasedEventHandler(
    ILogger<LiveStreamProductShowcasedEventHandler> logger) : INotificationHandler<LiveStreamProductShowcasedEvent>
{
    public async Task Handle(LiveStreamProductShowcasedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream product showcased event received. StreamId: {StreamId}, ProductId: {ProductId}, ShowcasedAt: {ShowcasedAt}",
            notification.StreamId, notification.ProductId, notification.ShowcasedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Real-time notification gönderimi (SignalR, WebSocket) - product showcased
            // - Analytics tracking (product showcase metrics, viewer engagement)
            // - Cache invalidation (highlighted products cache)
            // - External system integration (streaming platform API - highlight product)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamProductShowcasedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
