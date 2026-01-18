using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class EmailVerificationCreatedEventHandler(ILogger<EmailVerificationCreatedEventHandler> logger) : INotificationHandler<EmailVerificationCreatedEvent>
{
    public async Task Handle(EmailVerificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EmailVerification created event received. VerificationId: {VerificationId}, UserId: {UserId}, Email: {Email}",
            notification.VerificationId, notification.UserId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email gönderimi (verification email)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
