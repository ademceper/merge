using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Subscription.EventHandlers;

/// <summary>
/// SubscriptionPayment Completed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class SubscriptionPaymentCompletedEventHandler : INotificationHandler<SubscriptionPaymentCompletedEvent>
{
    private readonly ILogger<SubscriptionPaymentCompletedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public SubscriptionPaymentCompletedEventHandler(
        ILogger<SubscriptionPaymentCompletedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(SubscriptionPaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling SubscriptionPaymentCompletedEvent. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}",
                notification.PaymentId, notification.UserSubscriptionId);
            throw;
        }
    }
}
