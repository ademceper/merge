using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPayment Retried Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPaymentRetriedEventHandler : INotificationHandler<SubscriptionPaymentRetriedEvent>
{
    private readonly ILogger<SubscriptionPaymentRetriedEventHandler> _logger;

    public SubscriptionPaymentRetriedEventHandler(ILogger<SubscriptionPaymentRetriedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SubscriptionPaymentRetriedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SubscriptionPaymentRetriedEvent. PaymentId: {PaymentId}, UserSubscriptionId: {UserSubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
