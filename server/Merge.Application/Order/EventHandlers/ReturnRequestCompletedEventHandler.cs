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


public class ReturnRequestCompletedEventHandler(ILogger<ReturnRequestCompletedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<ReturnRequestCompletedEvent>
{
    public async Task Handle(ReturnRequestCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Return request completed event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, TrackingNumber: {TrackingNumber}, CompletedAt: {CompletedAt}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.TrackingNumber ?? "N/A", notification.CompletedAt);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Tamamlandı",
                    $"İade talebiniz tamamlandı. İade Talebi No: {notification.ReturnRequestId}, Tamamlanma Tarihi: {notification.CompletedAt:yyyy-MM-dd HH:mm:ss}" +
                    (string.IsNullOrEmpty(notification.TrackingNumber) ? "" : $", Takip No: {notification.TrackingNumber}"),
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestCompletedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReturnRequestCompletedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
