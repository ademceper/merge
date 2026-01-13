using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Cleared From Customs Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingClearedFromCustomsEventHandler : INotificationHandler<InternationalShippingClearedFromCustomsEvent>
{
    private readonly ILogger<InternationalShippingClearedFromCustomsEventHandler> _logger;

    public InternationalShippingClearedFromCustomsEventHandler(ILogger<InternationalShippingClearedFromCustomsEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingClearedFromCustomsEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping cleared from customs event received. ShippingId: {ShippingId}, OrderId: {OrderId}",
            notification.InternationalShippingId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo gümrükten çıktı bildirimi)
            // - Analytics tracking (customs clearance metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingClearedFromCustomsEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
