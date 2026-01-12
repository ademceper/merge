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
/// UserSubscription Suspended Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserSubscriptionSuspendedEventHandler : INotificationHandler<UserSubscriptionSuspendedEvent>
{
    private readonly ILogger<UserSubscriptionSuspendedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public UserSubscriptionSuspendedEventHandler(
        ILogger<UserSubscriptionSuspendedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(UserSubscriptionSuspendedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "User subscription suspended event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
            notification.SubscriptionId, notification.UserId);

        try
        {
            // Email gönderimi
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserSubscriptionSuspendedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
