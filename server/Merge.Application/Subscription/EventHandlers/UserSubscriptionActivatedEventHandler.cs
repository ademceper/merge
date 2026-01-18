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


public class UserSubscriptionActivatedEventHandler(ILogger<UserSubscriptionActivatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionActivatedEvent>
{
    public async Task Handle(UserSubscriptionActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User subscription activated event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
            notification.SubscriptionId, notification.UserId);

        try
        {
            // Email gönderimi
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Account,
                    "Abonelik Aktifleştirildi",
                    "Aboneliğiniz başarıyla aktifleştirildi."
                ), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Activation email gönderimi
            // - Analytics tracking (subscription activation metrics)
            // - Cache invalidation (user subscriptions cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserSubscriptionActivatedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
