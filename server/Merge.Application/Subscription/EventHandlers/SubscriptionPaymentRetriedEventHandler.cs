using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;


public class SubscriptionPaymentRetriedEventHandler(ILogger<SubscriptionPaymentRetriedEventHandler> logger) : INotificationHandler<SubscriptionPaymentRetriedEvent>
{

    public async Task Handle(SubscriptionPaymentRetriedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscription payment retried event received. PaymentId: {PaymentId}, UserSubscriptionId: {UserSubscriptionId}, RetryCount: {RetryCount}",
            notification.PaymentId, notification.UserSubscriptionId, notification.RetryCount);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (payment retry metrics)
            // - Email bildirimi (kullanıcıya ödeme tekrar denendi bildirimi)
            // - External system integration (payment gateway, CRM)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SubscriptionPaymentRetriedEvent. PaymentId: {PaymentId}, UserSubscriptionId: {UserSubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
