using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Purchase Order Cancelled Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PurchaseOrderCancelledEventHandler(
    ILogger<PurchaseOrderCancelledEventHandler> logger) : INotificationHandler<PurchaseOrderCancelledEvent>
{

    public async Task Handle(PurchaseOrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Purchase order cancelled event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, PONumber: {PONumber}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.PONumber);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email notification
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP)

        await Task.CompletedTask;
    }
}
