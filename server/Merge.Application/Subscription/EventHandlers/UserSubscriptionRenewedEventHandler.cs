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
/// UserSubscription Renewed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserSubscriptionRenewedEventHandler(ILogger<UserSubscriptionRenewedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<UserSubscriptionRenewedEvent>
{
    
    private readonly INotificationService? _notificationService;

    public async Task Handle(UserSubscriptionRenewedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "User subscription renewed event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, NewEndDate: {NewEndDate}, RenewalCount: {RenewalCount}",
            notification.SubscriptionId, notification.UserId, notification.NewEndDate, notification.RenewalCount);

        try
        {
            // Email gönderimi
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling UserSubscriptionRenewedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
