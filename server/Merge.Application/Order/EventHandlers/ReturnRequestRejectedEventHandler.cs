using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;


public class ReturnRequestRejectedEventHandler(ILogger<ReturnRequestRejectedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<ReturnRequestRejectedEvent>
{
    public async Task Handle(ReturnRequestRejectedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Return request rejected event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, RejectionReason: {RejectionReason}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.RejectionReason ?? "N/A");

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Reddedildi",
                    $"İade talebiniz reddedildi. İade Talebi No: {notification.ReturnRequestId}. Sebep: {notification.RejectionReason ?? "Belirtilmedi"}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestRejectedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReturnRequestRejectedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
