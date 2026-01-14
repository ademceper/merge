using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Product Added Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamProductAddedEventHandler(
    ILogger<LiveStreamProductAddedEventHandler> logger) : INotificationHandler<LiveStreamProductAddedEvent>
{
    public async Task Handle(LiveStreamProductAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream product added event received. StreamId: {StreamId}, ProductId: {ProductId}, SpecialPrice: {SpecialPrice}",
            notification.StreamId, notification.ProductId, notification.SpecialPrice);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (stream products cache)
            // - Search index update (Elasticsearch, Algolia) - product added to stream
            // - Analytics tracking (product addition metrics)
            // - Notification gönderimi (followers'a yeni ürün eklendi bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamProductAddedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
