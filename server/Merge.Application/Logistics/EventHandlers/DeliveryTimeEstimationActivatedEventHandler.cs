using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// DeliveryTimeEstimation Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class DeliveryTimeEstimationActivatedEventHandler(
    ILogger<DeliveryTimeEstimationActivatedEventHandler> logger) : INotificationHandler<DeliveryTimeEstimationActivatedEvent>
{

    public async Task Handle(DeliveryTimeEstimationActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "DeliveryTimeEstimation activated event received. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
            notification.DeliveryTimeEstimationId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active delivery time estimations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling DeliveryTimeEstimationActivatedEvent. DeliveryTimeEstimationId: {DeliveryTimeEstimationId}",
                notification.DeliveryTimeEstimationId);
            throw;
        }
    }
}
