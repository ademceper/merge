using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Loyalty Account Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class LoyaltyAccountCreatedEventHandler : INotificationHandler<LoyaltyAccountCreatedEvent>
{
    private readonly ILogger<LoyaltyAccountCreatedEventHandler> _logger;

    public LoyaltyAccountCreatedEventHandler(ILogger<LoyaltyAccountCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LoyaltyAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Loyalty account created event received. AccountId: {AccountId}, UserId: {UserId}",
            notification.AccountId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - Analytics tracking
        // - Initial tier assignment

        await Task.CompletedTask;
    }
}
