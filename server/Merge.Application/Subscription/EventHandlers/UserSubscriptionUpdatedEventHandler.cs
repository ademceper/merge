using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class UserSubscriptionUpdatedEventHandler(ILogger<UserSubscriptionUpdatedEventHandler> logger) : INotificationHandler<UserSubscriptionUpdatedEvent>
{

    public async Task Handle(UserSubscriptionUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling UserSubscriptionUpdatedEvent. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
                notification.SubscriptionId, notification.UserId);
            throw;
        }
    }
}
