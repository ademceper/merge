using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// EmailVerification Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class EmailVerificationCreatedEventHandler(ILogger<EmailVerificationCreatedEventHandler> logger) : INotificationHandler<EmailVerificationCreatedEvent>
{
    public async Task Handle(EmailVerificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "EmailVerification created event received. VerificationId: {VerificationId}, UserId: {UserId}, Email: {Email}",
            notification.VerificationId, notification.UserId, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Email gönderimi (verification email)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
