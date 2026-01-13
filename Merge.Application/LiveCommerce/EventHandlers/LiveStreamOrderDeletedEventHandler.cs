using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

/// <summary>
/// Live Stream Order Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class LiveStreamOrderDeletedEventHandler(
    ILogger<LiveStreamOrderDeletedEventHandler> logger) : INotificationHandler<LiveStreamOrderDeletedEvent>
{
    public async Task Handle(LiveStreamOrderDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Live stream order deleted event received. StreamId: {StreamId}, OrderId: {OrderId}, OrderAmount: {OrderAmount}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.OrderId, notification.OrderAmount, notification.DeletedAt);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (order deletion metrics, revenue adjustment)
            // - Cache invalidation (stream stats cache, order count cache)
            // - External system integration (order management system - order deleted)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling LiveStreamOrderDeletedEvent. StreamId: {StreamId}, OrderId: {OrderId}",
                notification.StreamId, notification.OrderId);
            throw;
        }
    }
}
