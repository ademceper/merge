using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Product Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamProductDeletedEventHandler(
    ILogger<LiveStreamProductDeletedEventHandler> logger) : INotificationHandler<LiveStreamProductDeletedEvent>
{
    public async Task Handle(LiveStreamProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream product deleted event received. StreamId: {StreamId}, ProductId: {ProductId}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.ProductId, notification.DeletedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (stream products cache)
            // - Search index update (Elasticsearch, Algolia) - product removed from stream
            // - Analytics tracking (product deletion metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamProductDeletedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
