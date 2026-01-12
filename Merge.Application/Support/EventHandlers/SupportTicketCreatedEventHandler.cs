using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketCreatedEventHandler : INotificationHandler<SupportTicketCreatedEvent>
{
    private readonly ILogger<SupportTicketCreatedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SupportTicketCreatedEventHandler(
        ILogger<SupportTicketCreatedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SupportTicketCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Support ticket created event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, Category: {Category}, Priority: {Priority}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.Category, notification.Priority);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Oluşturuldu",
                    $"Destek talebiniz başarıyla oluşturuldu. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketCreatedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SupportTicketCreatedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
