using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Viewer Left Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class ViewerLeftEventHandler(
    ILogger<ViewerLeftEventHandler> logger) : INotificationHandler<ViewerLeftEvent>
{
    public async Task Handle(ViewerLeftEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Viewer left event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, LeftAt: {LeftAt}, WatchDurationInSeconds: {WatchDurationInSeconds}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.LeftAt, notification.WatchDurationInSeconds);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Real-time notification gönderimi (SignalR, WebSocket) - viewer count update
            // - Analytics tracking (viewer retention metrics, average watch duration)
            // - Cache invalidation (stream viewer count cache)
            // - External system integration (streaming platform API - viewer count update)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ViewerLeftEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
