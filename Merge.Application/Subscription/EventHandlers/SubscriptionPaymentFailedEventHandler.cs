using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPayment Failed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPaymentFailedEventHandler : INotificationHandler<SubscriptionPaymentFailedEvent>
{
    private readonly ILogger<SubscriptionPaymentFailedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SubscriptionPaymentFailedEventHandler(
        ILogger<SubscriptionPaymentFailedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SubscriptionPaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogWarning(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SubscriptionPaymentFailedEvent. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
