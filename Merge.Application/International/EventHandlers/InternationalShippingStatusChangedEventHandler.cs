using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingStatusChangedEventHandler : INotificationHandler<InternationalShippingStatusChangedEvent>
{
    private readonly ILogger<InternationalShippingStatusChangedEventHandler> _logger;

    public InternationalShippingStatusChangedEventHandler(ILogger<InternationalShippingStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping status changed event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.InternationalShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo durumu değişikliği bildirimi)
            // - Analytics tracking (shipping status change metrics)
            // - External shipping provider status sync

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingStatusChangedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
