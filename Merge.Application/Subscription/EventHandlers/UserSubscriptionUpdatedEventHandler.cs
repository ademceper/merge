using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// UserSubscription Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserSubscriptionUpdatedEventHandler : INotificationHandler<UserSubscriptionUpdatedEvent>
{
    private readonly ILogger<UserSubscriptionUpdatedEventHandler> _logger;

    public UserSubscriptionUpdatedEventHandler(ILogger<UserSubscriptionUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserSubscriptionUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "User subscription updated event received. SubscriptionId: {SubscriptionId}, UserId: {UserId}, AutoRenewChanged: {AutoRenewChanged}, PaymentMethodChanged: {PaymentMethodChanged}",
            notification.SubscriptionId, notification.UserId, notification.AutoRenewChanged, notification.PaymentMethodChanged);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user subscription cache)
            // - Analytics tracking (subscription settings changed metrics)
            // - Email bildirimi (kullanıcıya ayar değişikliği bildirimi)
            // - External system integration (CRM, marketing tools)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserSubscriptionUpdatedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
