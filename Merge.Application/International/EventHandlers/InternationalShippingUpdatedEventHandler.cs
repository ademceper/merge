using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingUpdatedEventHandler : INotificationHandler<InternationalShippingUpdatedEvent>
{
    private readonly ILogger<InternationalShippingUpdatedEventHandler> _logger;

    public InternationalShippingUpdatedEventHandler(ILogger<InternationalShippingUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping updated event received. ShippingId: {ShippingId}, OrderId: {OrderId}, UpdateType: {UpdateType}",
            notification.InternationalShippingId, notification.OrderId, notification.UpdateType);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo detayları güncellendi bildirimi)
            // - Analytics tracking (shipping update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
