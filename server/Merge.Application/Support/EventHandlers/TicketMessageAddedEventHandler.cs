using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Support.EventHandlers;


public class TicketMessageAddedEventHandler(
    ILogger<TicketMessageAddedEventHandler> logger,
    INotificationService? notificationService) : INotificationHandler<TicketMessageAddedEvent>
{

    public async Task Handle(TicketMessageAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Ticket message added event received. MessageId: {MessageId}, TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, IsStaffResponse: {IsStaffResponse}",
            notification.MessageId, notification.TicketId, notification.TicketNumber, notification.UserId, notification.IsStaffResponse);

        try
        {
            // Eğer staff response ise kullanıcıya, değilse staff'a bildirim gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    notification.IsStaffResponse ? "Destek Talebine Yanıt Geldi" : "Yeni Mesaj",
                    notification.IsStaffResponse 
                        ? $"Destek talebinize yeni bir yanıt geldi. Talep No: {notification.TicketNumber}"
                        : $"Destek talebinize yeni bir mesaj eklendi. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackTicketMessageAddedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling TicketMessageAddedEvent. MessageId: {MessageId}, TicketId: {TicketId}, TicketNumber: {TicketNumber}",
                notification.MessageId, notification.TicketId, notification.TicketNumber);
            throw;
        }
    }
}
