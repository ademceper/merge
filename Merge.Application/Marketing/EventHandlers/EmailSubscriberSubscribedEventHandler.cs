using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Email Subscriber Subscribed Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailSubscriberSubscribedEventHandler : INotificationHandler<EmailSubscriberSubscribedEvent>
{
    private readonly ILogger<EmailSubscriberSubscribedEventHandler> _logger;

    public EmailSubscriberSubscribedEventHandler(ILogger<EmailSubscriberSubscribedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailSubscriberSubscribedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email subscriber subscribed event received. SubscriberId: {SubscriberId}, Email: {Email}",
            notification.SubscriberId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - Analytics tracking
        // - External email service sync

        await Task.CompletedTask;
    }
}
