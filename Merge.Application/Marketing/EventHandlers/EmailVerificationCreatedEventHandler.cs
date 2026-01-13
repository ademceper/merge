using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailVerification Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class EmailVerificationCreatedEventHandler : INotificationHandler<EmailVerificationCreatedEvent>
{
    private readonly ILogger<EmailVerificationCreatedEventHandler> _logger;

    public EmailVerificationCreatedEventHandler(ILogger<EmailVerificationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EmailVerificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "EmailVerification created event received. VerificationId: {VerificationId}, UserId: {UserId}, Email: {Email}",
            notification.VerificationId, notification.UserId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email gönderimi (verification email)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
