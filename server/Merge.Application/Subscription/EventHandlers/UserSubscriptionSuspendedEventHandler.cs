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


public class UserSubscriptionSuspendedEventHandler(ILogger<UserSubscriptionSuspendedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionSuspendedEvent>
{
    public async Task Handle(UserSubscriptionSuspendedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User subscription suspended event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
            notification.SubscriptionId, notification.UserId);

        try
        {
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Account,
                    "Abonelik Askıya Alındı",
                    "Aboneliğiniz askıya alındı. Lütfen destek ekibi ile iletişime geçin."
                ), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Suspension email gönderimi
            // - Analytics tracking (subscription suspension metrics)
            // - Cache invalidation (user subscriptions cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserSubscriptionSuspendedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
