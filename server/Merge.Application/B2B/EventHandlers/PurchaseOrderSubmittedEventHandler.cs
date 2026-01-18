using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class PurchaseOrderSubmittedEventHandler(
    ILogger<PurchaseOrderSubmittedEventHandler> logger,
    INotificationService? notificationService = null) : INotificationHandler<PurchaseOrderSubmittedEvent>
{

    public async Task Handle(PurchaseOrderSubmittedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Purchase order submitted event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, PONumber: {PONumber}, TotalAmount: {TotalAmount}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.PONumber, notification.TotalAmount);

        try
        {
            // Email bildirimi gönder (admin'lere)
            if (notificationService != null)
            {
                // TODO: Admin user ID'lerini al ve notification gönder
                // var adminUsers = await _userService.GetAdminUsersAsync(cancellationToken);
                // foreach (var admin in adminUsers)
                // {
                //     var createDto = new CreateNotificationDto(
                //         admin.Id,
                //         NotificationType.Order,
                //         "Yeni Satın Alma Siparişi",
                //         $"Yeni bir satın alma siparişi gönderildi. PO No: {notification.PONumber}",
                //         null,
                //         null);
                //     await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
                // }
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Email notification (admin'lere)
            // - Analytics tracking
            // - Audit log
            // - External system sync (ERP)
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling PurchaseOrderSubmittedEvent. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                notification.PurchaseOrderId, notification.PONumber);
            throw;
        }
    }
}
