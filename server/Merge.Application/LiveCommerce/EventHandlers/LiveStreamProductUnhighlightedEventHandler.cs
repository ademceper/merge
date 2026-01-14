using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Product Unhighlighted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamProductUnhighlightedEventHandler(
    ILogger<LiveStreamProductUnhighlightedEventHandler> logger) : INotificationHandler<LiveStreamProductUnhighlightedEvent>
{
    public async Task Handle(LiveStreamProductUnhighlightedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream product unhighlighted event received. StreamId: {StreamId}, ProductId: {ProductId}, UnhighlightedAt: {UnhighlightedAt}",
            notification.StreamId, notification.ProductId, notification.UnhighlightedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Real-time notification gönderimi (SignalR, WebSocket) - product unhighlighted
            // - Analytics tracking (product unhighlight metrics)
            // - Cache invalidation (highlighted products cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamProductUnhighlightedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
