using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Referral Code Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class ReferralCodeCreatedEventHandler : INotificationHandler<ReferralCodeCreatedEvent>
{
    private readonly ILogger<ReferralCodeCreatedEventHandler> _logger;

    public ReferralCodeCreatedEventHandler(ILogger<ReferralCodeCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReferralCodeCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Referral code created event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}",
            notification.ReferralCodeId, notification.UserId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (referral code ready)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
