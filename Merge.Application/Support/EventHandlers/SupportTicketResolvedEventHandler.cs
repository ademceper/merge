using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Support.EventHandlers;

/// <summary>
/// Support Ticket Resolved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SupportTicketResolvedEventHandler : INotificationHandler<SupportTicketResolvedEvent>
{
    private readonly ILogger<SupportTicketResolvedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SupportTicketResolvedEventHandler(
        ILogger<SupportTicketResolvedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SupportTicketResolvedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Support ticket resolved event received. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}, ResolvedAt: {ResolvedAt}",
            notification.TicketId, notification.TicketNumber, notification.UserId, notification.ResolvedAt);

        try
        {
            // Kullanıcıya bildirim gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Support,
                    "Destek Talebi Çözüldü",
                    $"Destek talebiniz çözüldü. Talep No: {notification.TicketNumber}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackSupportTicketResolvedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SupportTicketResolvedEvent. TicketId: {TicketId}, TicketNumber: {TicketNumber}, UserId: {UserId}",
                notification.TicketId, notification.TicketNumber, notification.UserId);
            throw;
        }
    }
}
