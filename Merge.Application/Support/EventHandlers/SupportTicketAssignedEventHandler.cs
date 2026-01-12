using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Assigned Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketAssignedEventHandler : INotificationHandler<SupportTicketAssignedEvent>
{
    private readonly ILogger<SupportTicketAssignedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SupportTicketAssignedEventHandler(
        ILogger<SupportTicketAssignedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SupportTicketAssignedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Support ticket assigned event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, AssignedToId: {AssignedToId}",
            notification.TicketId, notification.TicketNumber, notification.AssignedToId);

        try
        {
            // Agent'a bildirim gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.AssignedToId,
                    NotificationType.Support,
                    "Yeni Destek Talebi Atandı",
                    $"Size yeni bir destek talebi atandı. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketAssignedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SupportTicketAssignedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, AssignedToId: {AssignedToId}",
                notification.TicketId, notification.TicketNumber, notification.AssignedToId);
            throw;
        }
    }
}
