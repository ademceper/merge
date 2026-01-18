using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailSubscriberSubscribedEventHandler(ILogger<EmailSubscriberSubscribedEventHandler> logger) : INotificationHandler<EmailSubscriberSubscribedEvent>
{
    public async Task Handle(EmailSubscriberSubscribedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email subscriber subscribed event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - Analytics tracking
        // - External email service sync

        await Task.CompletedTask;
    }
}
