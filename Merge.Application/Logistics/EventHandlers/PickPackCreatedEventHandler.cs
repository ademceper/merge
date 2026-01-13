using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// PickPack Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PickPackCreatedEventHandler : INotificationHandler<PickPackCreatedEvent>
{
    private readonly ILogger<PickPackCreatedEventHandler> _logger;

    public PickPackCreatedEventHandler(ILogger<PickPackCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PickPackCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "PickPack created event received. PickPackId: {PickPackId}, OrderId: {OrderId}, WarehouseId: {WarehouseId}, PackNumber: {PackNumber}",
            notification.PickPackId, notification.OrderId, notification.WarehouseId, notification.PackNumber);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (warehouse staff'a yeni pick-pack bildirimi)
            // - Analytics tracking (pick-pack creation metrics)
            // - Cache invalidation (warehouse pick-pack stats cache)
            // - External system integration (WMS, ERP)
            // - Email notification (warehouse manager'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling PickPackCreatedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}, PackNumber: {PackNumber}",
                notification.PickPackId, notification.OrderId, notification.PackNumber);
            throw;
        }
    }
}
