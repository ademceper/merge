using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPaymentFailedEventHandler(ILogger<SubscriptionPaymentFailedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SubscriptionPaymentFailedEvent>
{
    public async Task Handle(SubscriptionPaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Subscription payment failed event received. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}, Amount: {Amount}, Reason: {Reason}",
            notification.PaymentId, notification.UserSubscriptionId, notification.Amount, notification.Reason);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Payment failure email gönderimi
            // - Retry payment scheduling
            // - Analytics tracking (payment failure metrics)
            // - Alert to admin (if critical)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPaymentFailedEvent. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
