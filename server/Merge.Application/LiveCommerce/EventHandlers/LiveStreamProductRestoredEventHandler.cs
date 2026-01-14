using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Product Restored Event Handler - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamProductRestoredEventHandler(
    ILogger<LiveStreamProductRestoredEventHandler> logger) : INotificationHandler<LiveStreamProductRestoredEvent>
{
    public async Task Handle(LiveStreamProductRestoredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream product restored event received. StreamId: {StreamId}, ProductId: {ProductId}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.ProductId, notification.RestoredAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (deleted products cache)
            // - Search index update (Elasticsearch, Algolia) - product restored to stream
            // - Analytics tracking (product restore metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamProductRestoredEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
