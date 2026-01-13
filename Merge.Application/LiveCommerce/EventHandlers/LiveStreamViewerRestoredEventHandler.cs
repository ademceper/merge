using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Viewer Restored Event Handler - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamViewerRestoredEventHandler(
    ILogger<LiveStreamViewerRestoredEventHandler> logger) : INotificationHandler<LiveStreamViewerRestoredEvent>
{
    public async Task Handle(LiveStreamViewerRestoredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream viewer restored event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.RestoredAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (viewer restore metrics)
            // - Cache invalidation (deleted viewers cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamViewerRestoredEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
