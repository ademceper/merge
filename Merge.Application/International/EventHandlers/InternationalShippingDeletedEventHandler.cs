using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingDeletedEventHandler : INotificationHandler<InternationalShippingDeletedEvent>
{
    private readonly ILogger<InternationalShippingDeletedEventHandler> _logger;

    public InternationalShippingDeletedEventHandler(ILogger<InternationalShippingDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping deleted event received. ShippingId: {ShippingId}, OrderId: {OrderId}",
            notification.InternationalShippingId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (shipping deletion metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingDeletedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
