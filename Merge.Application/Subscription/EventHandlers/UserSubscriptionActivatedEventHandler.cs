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
/// UserSubscription Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserSubscriptionActivatedEventHandler : INotificationHandler<UserSubscriptionActivatedEvent>
{
    private readonly ILogger<UserSubscriptionActivatedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public UserSubscriptionActivatedEventHandler(
        ILogger<UserSubscriptionActivatedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(UserSubscriptionActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "User subscription activated event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
            notification.SubscriptionId, notification.UserId);

        try
        {
            // Email gönderimi
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserSubscriptionActivatedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
