using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Return Request Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ReturnRequestCreatedEventHandler : INotificationHandler<ReturnRequestCreatedEvent>
{
    private readonly ILogger<ReturnRequestCreatedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public ReturnRequestCreatedEventHandler(
        ILogger<ReturnRequestCreatedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(ReturnRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Return request created event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, RefundAmount: {RefundAmount}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.RefundAmount);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notification.UserId,
                    "İade Talebi Oluşturuldu",
                    $"İade talebiniz başarıyla oluşturuldu. İade Talebi No: {notification.ReturnRequestId}, İade Tutarı: {notification.RefundAmount} TL",
                    "ReturnRequest",
                    cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestCreatedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ReturnRequestCreatedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
