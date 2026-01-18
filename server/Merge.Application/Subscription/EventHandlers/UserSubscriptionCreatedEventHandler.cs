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


public class UserSubscriptionCreatedEventHandler(ILogger<UserSubscriptionCreatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionCreatedEvent>
{
    public async Task Handle(UserSubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User subscription created event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, PlanId: {PlanId}, Status: {Status}, Price: {Price}",
            notification.SubscriptionId, notification.UserId, notification.PlanId, notification.Status, notification.Price);

        try
        {
            // Email gönderimi
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Account,
                    "Abonelik Oluşturuldu",
                    $"Aboneliğiniz başarıyla oluşturuldu. Durum: {notification.Status}"
                ), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Welcome email gönderimi
            // - Analytics tracking (subscription creation metrics)
            // - Cache invalidation (user subscriptions cache)
            // - External system integration (CRM, marketing tools)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserSubscriptionCreatedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
