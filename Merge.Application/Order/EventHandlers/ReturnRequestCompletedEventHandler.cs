using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Return Request Completed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReturnRequestCompletedEventHandler : INotificationHandler<ReturnRequestCompletedEvent>
{
    private readonly ILogger<ReturnRequestCompletedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public ReturnRequestCompletedEventHandler(
        ILogger<ReturnRequestCompletedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(ReturnRequestCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Return request completed event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, TrackingNumber: {TrackingNumber}, CompletedAt: {CompletedAt}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.TrackingNumber ?? "N/A", notification.CompletedAt);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Tamamlandı",
                    $"İade talebiniz tamamlandı. İade Talebi No: {notification.ReturnRequestId}, Tamamlanma Tarihi: {notification.CompletedAt:yyyy-MM-dd HH:mm:ss}" +
                    (string.IsNullOrEmpty(notification.TrackingNumber) ? "" : $", Takip No: {notification.TrackingNumber}"),
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestCompletedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReturnRequestCompletedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
