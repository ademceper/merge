using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailSubscriberUnsubscribedEventHandler(ILogger<EmailSubscriberUnsubscribedEventHandler> logger) : INotificationHandler<EmailSubscriberUnsubscribedEvent>
{
    public async Task Handle(EmailSubscriberUnsubscribedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email subscriber unsubscribed event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - External email service sync (unsubscribe)
        // - Analytics tracking
        // - Feedback collection

        await Task.CompletedTask;
    }
}
