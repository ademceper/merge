using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ShippingCreatedEventHandler : INotificationHandler<ShippingCreatedEvent>
{
    private readonly ILogger<ShippingCreatedEventHandler> _logger;

    public ShippingCreatedEventHandler(ILogger<ShippingCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ShippingCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Shipping created event received. ShippingId: {ShippingId}, OrderId: {OrderId}, ShippingProvider: {ShippingProvider}, ShippingCost: {ShippingCost}",
            notification.ShippingId, notification.OrderId, notification.ShippingProvider, notification.ShippingCost);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo oluşturuldu bildirimi)
            // - Analytics tracking (shipping creation metrics)
            // - External shipping provider integration (tracking number generation)
            // - Email notification (order shipped email)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ShippingCreatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, ShippingProvider: {ShippingProvider}",
                notification.ShippingId, notification.OrderId, notification.ShippingProvider);
            throw;
        }
    }
}
