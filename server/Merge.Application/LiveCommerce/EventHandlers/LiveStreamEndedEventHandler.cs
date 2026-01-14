using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Ended Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamEndedEventHandler(
    ILogger<LiveStreamEndedEventHandler> logger) : INotificationHandler<LiveStreamEndedEvent>
{
    public async Task Handle(LiveStreamEndedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream ended event received. StreamId: {StreamId}, SellerId: {SellerId}, EndedAt: {EndedAt}, TotalViewerCount: {TotalViewerCount}, OrderCount: {OrderCount}, Revenue: {Revenue}",
            notification.StreamId, notification.SellerId, notification.EndedAt, notification.TotalViewerCount, notification.OrderCount, notification.Revenue);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (stream performance metrics, revenue analysis)
            // - Report generation (stream summary report)
            // - Cache invalidation (active streams cache, stream stats cache)
            // - External system integration (streaming platform API - end stream)
            // - Email notification (stream summary report to seller)
            // - Notification gönderimi (seller'a stream sonlandı bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamEndedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
