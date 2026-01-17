using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// UserSubscription Cancelled Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserSubscriptionCancelledEventHandler(ILogger<UserSubscriptionCancelledEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionCancelledEvent>
{
    
    private readonly INotificationService? _notificationService;

    public async Task Handle(UserSubscriptionCancelledEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "User subscription cancelled event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, Reason: {Reason}",
            notification.SubscriptionId, notification.UserId, notification.Reason);

        try
        {
            // Email gönderimi
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Account,
                    "Abonelik İptal Edildi",
                    $"Aboneliğiniz iptal edildi.{(string.IsNullOrEmpty(notification.Reason) ? "" : $" Sebep: {notification.Reason}")}"
                ), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Cancellation email gönderimi
            // - Analytics tracking (subscription cancellation metrics)
            // - Cache invalidation (user subscriptions cache)
            // - Refund processing (eğer gerekirse)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling UserSubscriptionCancelledEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
