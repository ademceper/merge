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


public class UserSubscriptionCancelledEventHandler(ILogger<UserSubscriptionCancelledEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionCancelledEvent>
{
    public async Task Handle(UserSubscriptionCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User subscription cancelled event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, Reason: {Reason}",
            notification.SubscriptionId, notification.UserId, notification.Reason);

        try
        {
            // Email gönderimi
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
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
            logger.LogError(ex,
                "Error handling UserSubscriptionCancelledEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
