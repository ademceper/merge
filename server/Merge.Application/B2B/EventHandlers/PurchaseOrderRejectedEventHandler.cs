using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class PurchaseOrderRejectedEventHandler(
    ILogger<PurchaseOrderRejectedEventHandler> logger,
    INotificationService? notificationService = null) : INotificationHandler<PurchaseOrderRejectedEvent>
{

    public async Task Handle(PurchaseOrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Purchase order rejected event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, PONumber: {PONumber}, Reason: {Reason}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.PONumber, notification.Reason);

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
                //         "Satın Alma Siparişi Reddedildi",
                //         $"Satın alma siparişiniz reddedildi. PO No: {notification.PONumber}, Sebep: {notification.Reason}",
                //         null,
                //         null);
                //     await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
                // }
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Email notification (B2B kullanıcıya)
            // - Analytics tracking
            // - Audit log
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling PurchaseOrderRejectedEvent. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                notification.PurchaseOrderId, notification.PONumber);
            throw;
        }
    }
}
