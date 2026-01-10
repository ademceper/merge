using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// ReferralCode Used Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class ReferralCodeUsedEventHandler : INotificationHandler<ReferralCodeUsedEvent>
{
    private readonly ILogger<ReferralCodeUsedEventHandler> _logger;

    public ReferralCodeUsedEventHandler(ILogger<ReferralCodeUsedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReferralCodeUsedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Referral code used event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}, UsageCount: {UsageCount}, MaxUsage: {MaxUsage}",
            notification.ReferralCodeId, notification.UserId, notification.Code, notification.UsageCount, notification.MaxUsage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Usage limit kontrolü
        // - Notification gönderimi (milestone achievements)

        await Task.CompletedTask;
    }
}
