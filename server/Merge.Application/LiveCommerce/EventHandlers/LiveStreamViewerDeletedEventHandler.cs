using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Viewer Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamViewerDeletedEventHandler(
    ILogger<LiveStreamViewerDeletedEventHandler> logger) : INotificationHandler<LiveStreamViewerDeletedEvent>
{
    public async Task Handle(LiveStreamViewerDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream viewer deleted event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.DeletedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (viewer deletion metrics)
            // - Cache invalidation (viewer count cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamViewerDeletedEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
