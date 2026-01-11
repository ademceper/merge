using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Return Request Approved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReturnRequestApprovedEventHandler : INotificationHandler<ReturnRequestApprovedEvent>
{
    private readonly ILogger<ReturnRequestApprovedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public ReturnRequestApprovedEventHandler(
        ILogger<ReturnRequestApprovedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(ReturnRequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Return request approved event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, ApprovedAt: {ApprovedAt}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.ApprovedAt);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Onaylandı",
                    $"İade talebiniz onaylandı. İade Talebi No: {notification.ReturnRequestId}, Onay Tarihi: {notification.ApprovedAt:yyyy-MM-dd HH:mm:ss}",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestApprovedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReturnRequestApprovedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
