using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class PurchaseOrderCreatedEventHandler(
    ILogger<PurchaseOrderCreatedEventHandler> logger) : INotificationHandler<PurchaseOrderCreatedEvent>
{

    public async Task Handle(PurchaseOrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Purchase order created event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, B2BUserId: {B2BUserId}, PONumber: {PONumber}, TotalAmount: {TotalAmount}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.B2BUserId, notification.PONumber, notification.TotalAmount);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email notification (draft PO created)
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP)

        await Task.CompletedTask;
    }
}
