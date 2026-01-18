using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

public class SubscriptionPaymentCompletedEventHandler(ILogger<SubscriptionPaymentCompletedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<SubscriptionPaymentCompletedEvent>
{
    public async Task Handle(SubscriptionPaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription payment completed event received. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}, Amount: {Amount}, TransactionId: {TransactionId}",
            notification.PaymentId, notification.UserSubscriptionId, notification.Amount, notification.TransactionId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Payment confirmation email gönderimi
            // - Invoice generation
            // - Analytics tracking (payment completion metrics)
            // - Cache invalidation (subscription payments cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPaymentCompletedEvent. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
