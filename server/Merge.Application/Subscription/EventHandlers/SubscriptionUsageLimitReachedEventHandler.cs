using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionUsageLimitReachedEventHandler(ILogger<SubscriptionUsageLimitReachedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SubscriptionUsageLimitReachedEvent>
{
    public async Task Handle(SubscriptionUsageLimitReachedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Subscription usage limit reached event received. UsageId: {UsageId}, UserSubscriptionId: {UserSubscriptionId}, UserId: {UserId}, Feature: {Feature}, UsageCount: {UsageCount}, Limit: {Limit}",
            notification.UsageId, notification.UserSubscriptionId, notification.UserId, notification.Feature, notification.UsageCount, notification.Limit);

        try
        {
            if (notificationService is not null)
            {
                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Warning,
                    "Kullanım Limiti Aşıldı",
                    $"{notification.Feature} özelliği için kullanım limitinize ulaştınız. Limit: {notification.Limit}, Kullanım: {notification.UsageCount}",
                    null,
                    null), cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (usage limit reached metrics)
            // - Email bildirimi (kullanıcıya limit aşıldı email'i)
            // - Upgrade prompt (premium plan'a geçiş önerisi)
            // - External system integration (CRM, marketing tools)
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionUsageLimitReachedEvent. UsageId: {UsageId}, UserSubscriptionId: {UserSubscriptionId}",
                notification.UsageId, notification.UserSubscriptionId);
            throw;
        }
    }
}
