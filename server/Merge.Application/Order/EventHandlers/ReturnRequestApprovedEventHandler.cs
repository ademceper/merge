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


public class ReturnRequestApprovedEventHandler(ILogger<ReturnRequestApprovedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<ReturnRequestApprovedEvent>
{
    public async Task Handle(ReturnRequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Return request approved event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, ApprovedAt: {ApprovedAt}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.ApprovedAt);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Onaylandı",
                    $"İade talebiniz onaylandı. İade Talebi No: {notification.ReturnRequestId}, Onay Tarihi: {notification.ApprovedAt:yyyy-MM-dd HH:mm:ss}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestApprovedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReturnRequestApprovedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
