using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Closed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketClosedEventHandler : INotificationHandler<SupportTicketClosedEvent>
{
    private readonly ILogger<SupportTicketClosedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SupportTicketClosedEventHandler(
        ILogger<SupportTicketClosedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SupportTicketClosedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Support ticket closed event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, ClosedAt: {ClosedAt}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.ClosedAt);

        try
        {
            // Kullanıcıya bildirim gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Kapatıldı",
                    $"Destek talebiniz kapatıldı. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketClosedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SupportTicketClosedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
