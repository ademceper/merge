using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailSubscriber Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailSubscriberCreatedEventHandler : INotificationHandler<EmailSubscriberCreatedEvent>
{
    private readonly ILogger<EmailSubscriberCreatedEventHandler> _logger;

    public EmailSubscriberCreatedEventHandler(ILogger<EmailSubscriberCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailSubscriberCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email subscriber created event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - External email service sync
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
