using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailSubscriber Unsubscribed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailSubscriberUnsubscribedEventHandler : INotificationHandler<EmailSubscriberUnsubscribedEvent>
{
    private readonly ILogger<EmailSubscriberUnsubscribedEventHandler> _logger;

    public EmailSubscriberUnsubscribedEventHandler(ILogger<EmailSubscriberUnsubscribedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailSubscriberUnsubscribedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email subscriber unsubscribed event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - External email service sync (unsubscribe)
        // - Analytics tracking
        // - Feedback collection

        await Task.CompletedTask;
    }
}
