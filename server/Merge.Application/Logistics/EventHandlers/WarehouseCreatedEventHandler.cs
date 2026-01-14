using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Warehouse Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class WarehouseCreatedEventHandler(
    ILogger<WarehouseCreatedEventHandler> logger) : INotificationHandler<WarehouseCreatedEvent>
{

    public async Task Handle(WarehouseCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Warehouse created event received. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
            notification.WarehouseId, notification.Name, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (warehouses cache)
            // - Analytics tracking (warehouse creation metrics)
            // - External system integration (WMS, ERP)
            // - Notification gönderimi (admin'lere yeni depo oluşturuldu bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling WarehouseCreatedEvent. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
                notification.WarehouseId, notification.Name, notification.Code);
            throw;
        }
    }
}
