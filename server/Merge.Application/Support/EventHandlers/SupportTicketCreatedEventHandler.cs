using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class SupportTicketCreatedEventHandler(ILogger<SupportTicketCreatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SupportTicketCreatedEvent>
{
    public async Task Handle(SupportTicketCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Support ticket created event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, Category: {Category}, Priority: {Priority}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.Category, notification.Priority);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Oluşturuldu",
                    $"Destek talebiniz başarıyla oluşturuldu. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketCreatedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SupportTicketCreatedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
