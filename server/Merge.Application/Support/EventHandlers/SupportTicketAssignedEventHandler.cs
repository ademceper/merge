using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;

public class SupportTicketAssignedEventHandler(ILogger<SupportTicketAssignedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SupportTicketAssignedEvent>
{
    public async Task Handle(SupportTicketAssignedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket assigned event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, AssignedToId: {AssignedToId}",
            notification.TicketId, notification.TicketNumber, notification.AssignedToId);

        try
        {
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.AssignedToId,
                    NotificationType.Support,
                    "Yeni Destek Talebi Atandı",
                    $"Size yeni bir destek talebi atandı. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketAssignedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SupportTicketAssignedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, AssignedToId: {AssignedToId}",
                notification.TicketId, notification.TicketNumber, notification.AssignedToId);
            throw;
        }
    }
}
