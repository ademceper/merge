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


public class UserSubscriptionRenewedEventHandler(ILogger<UserSubscriptionRenewedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionRenewedEvent>
{
    public async Task Handle(UserSubscriptionRenewedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User subscription renewed event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, NewEndDate: {NewEndDate}, RenewalCount: {RenewalCount}",
            notification.SubscriptionId, notification.UserId, notification.NewEndDate, notification.RenewalCount);

        try
        {
            // Email gönderimi
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Account,
                    "Abonelik Yenilendi",
                    $"Aboneliğiniz başarıyla yenilendi. Yeni bitiş tarihi: {notification.NewEndDate:dd.MM.yyyy}"
                ), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Renewal confirmation email gönderimi
            // - Analytics tracking (subscription renewal metrics)
            // - Cache invalidation (user subscriptions cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserSubscriptionRenewedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
