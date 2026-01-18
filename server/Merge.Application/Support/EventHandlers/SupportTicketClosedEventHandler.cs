using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class SupportTicketClosedEventHandler(ILogger<SupportTicketClosedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SupportTicketClosedEvent>
{
    public async Task Handle(SupportTicketClosedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket closed event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, ClosedAt: {ClosedAt}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.ClosedAt);

        try
        {
            // Kullanıcıya bildirim gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Kapatıldı",
                    $"Destek talebiniz kapatıldı. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketClosedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SupportTicketClosedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
