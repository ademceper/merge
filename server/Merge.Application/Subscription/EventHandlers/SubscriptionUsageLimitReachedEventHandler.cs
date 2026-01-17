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

/// <summary>
/// SubscriptionUsage Limit Reached Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionUsageLimitReachedEventHandler(ILogger<SubscriptionUsageLimitReachedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SubscriptionUsageLimitReachedEvent>
{
    
    private readonly INotificationService? _notificationService;

    public async Task Handle(SubscriptionUsageLimitReachedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogWarning(
            "Subscription usage limit reached event received. UsageId: {UsageId}, UserSubscriptionId: {UserSubscriptionId}, UserId: {UserId}, Feature: {Feature}, UsageCount: {UsageCount}, Limit: {Limit}",
            notification.UsageId, notification.UserSubscriptionId, notification.UserId, notification.Feature, notification.UsageCount, notification.Limit);

        try
        {
            // ✅ NOTIFICATION: Kullanıcıya limit aşıldı bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling SubscriptionUsageLimitReachedEvent. UsageId: {UsageId}, UserSubscriptionId: {UserSubscriptionId}",
                notification.UsageId, notification.UserSubscriptionId);
            throw;
        }
    }
}
