using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Ticket Message Added Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class TicketMessageAddedEventHandler : INotificationHandler<TicketMessageAddedEvent>
{
    private readonly ILogger<TicketMessageAddedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public TicketMessageAddedEventHandler(
        ILogger<TicketMessageAddedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(TicketMessageAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Ticket message added event received. MessageId: {MessageId}, TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, IsStaffResponse: {IsStaffResponse}",
            notification.MessageId, notification.TicketId, notification.TicketNumber, notification.UserId, notification.IsStaffResponse);

        try
        {
            // Eğer staff response ise kullanıcıya, değilse staff'a bildirim gönder
            if (_notificationService != null)
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
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackTicketMessageAddedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling TicketMessageAddedEvent. MessageId: {MessageId}, TicketId: {TicketId}, TicketNumber: {TicketNumber}",
                notification.MessageId, notification.TicketId, notification.TicketNumber);
            throw;
        }
    }
}
