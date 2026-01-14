using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Purchase Order Approved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PurchaseOrderApprovedEventHandler(
    ILogger<PurchaseOrderApprovedEventHandler> logger,
    INotificationService? notificationService = null) : INotificationHandler<PurchaseOrderApprovedEvent>
{

    public async Task Handle(PurchaseOrderApprovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Purchase order approved event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, ApprovedByUserId: {ApprovedByUserId}, PONumber: {PONumber}, TotalAmount: {TotalAmount}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.ApprovedByUserId, notification.PONumber, notification.TotalAmount);

        try
        {
            // Email bildirimi gönder (B2B kullanıcıya)
            if (notificationService != null)
            {
                // TODO: B2B user ID'den user ID'yi al ve notification gönder
                // var b2bUser = await _b2bService.GetB2BUserByIdAsync(notification.B2BUserId, cancellationToken);
                // if (b2bUser != null)
                // {
                //     var createDto = new CreateNotificationDto(
                //         b2bUser.UserId,
                //         NotificationType.Order,
                //         "Satın Alma Siparişi Onaylandı",
                //         $"Satın alma siparişiniz onaylandı. PO No: {notification.PONumber}",
                //         null,
                //         null);
                //     await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
                // }
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Email notification (B2B kullanıcıya)
            // - Inventory reservation
            // - Analytics tracking
            // - Audit log
            // - External system sync (ERP, Inventory)
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling PurchaseOrderApprovedEvent. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                notification.PurchaseOrderId, notification.PONumber);
            throw;
        }
    }
}
