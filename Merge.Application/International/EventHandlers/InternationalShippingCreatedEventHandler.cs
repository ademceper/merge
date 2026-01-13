using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingCreatedEventHandler : INotificationHandler<InternationalShippingCreatedEvent>
{
    private readonly ILogger<InternationalShippingCreatedEventHandler> _logger;

    public InternationalShippingCreatedEventHandler(ILogger<InternationalShippingCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping created event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OriginCountry: {OriginCountry}, DestinationCountry: {DestinationCountry}, ShippingMethod: {ShippingMethod}, ShippingCost: {ShippingCost}",
            notification.InternationalShippingId, notification.OrderId, notification.OriginCountry, notification.DestinationCountry, notification.ShippingMethod, notification.ShippingCost);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya uluslararası kargo oluşturuldu bildirimi)
            // - Analytics tracking (international shipping metrics)
            // - Customs documentation generation
            // - External shipping provider integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingCreatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
