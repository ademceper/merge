using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class SupportTicketResolvedEventHandler(ILogger<SupportTicketResolvedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SupportTicketResolvedEvent>
{

    public async Task Handle(SupportTicketResolvedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket resolved event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, ResolvedAt: {ResolvedAt}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.ResolvedAt);

        try
        {
            // Kullanıcıya bildirim gönder
            if (notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Çözüldü",
                    $"Destek talebiniz çözüldü. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketResolvedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SupportTicketResolvedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
