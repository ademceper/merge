using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamDeletedEventHandler(
    ILogger<LiveStreamDeletedEventHandler> logger) : INotificationHandler<LiveStreamDeletedEvent>
{
    public async Task Handle(LiveStreamDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream deleted event received. StreamId: {StreamId}, SellerId: {SellerId}, Title: {Title}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.SellerId, notification.Title, notification.DeletedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active streams cache, stream details cache)
            // - Search index update (Elasticsearch, Algolia) - stream silindi
            // - Notification gönderimi (seller'a stream silindi bildirimi)
            // - Analytics tracking (stream deletion metrics)
            // - External system integration (streaming platform API - delete stream)
            // - Data cleanup (if needed - soft delete olduğu için cascade delete gerekmez)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamDeletedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
