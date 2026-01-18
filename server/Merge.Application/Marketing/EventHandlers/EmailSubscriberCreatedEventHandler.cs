using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailSubscriberCreatedEventHandler(ILogger<EmailSubscriberCreatedEventHandler> logger) : INotificationHandler<EmailSubscriberCreatedEvent>
{
    public async Task Handle(EmailSubscriberCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email subscriber created event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - External email service sync
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
