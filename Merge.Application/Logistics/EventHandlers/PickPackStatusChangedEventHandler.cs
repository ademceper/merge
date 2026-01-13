using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// PickPack Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PickPackStatusChangedEventHandler : INotificationHandler<PickPackStatusChangedEvent>
{
    private readonly ILogger<PickPackStatusChangedEventHandler> _logger;

    public PickPackStatusChangedEventHandler(ILogger<PickPackStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PickPackStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "PickPack status changed event received. PickPackId: {PickPackId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.PickPackId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (warehouse staff'a status değişikliği bildirimi)
            // - Analytics tracking (pick-pack status transition metrics)
            // - Cache invalidation (warehouse pick-pack stats cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling PickPackStatusChangedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                notification.PickPackId, notification.OrderId, notification.OldStatus, notification.NewStatus);
            throw;
        }
    }
}
