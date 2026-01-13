using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// ReferralCode Used Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class ReferralCodeUsedEventHandler(ILogger<ReferralCodeUsedEventHandler> logger) : INotificationHandler<ReferralCodeUsedEvent>
{
    public async Task Handle(ReferralCodeUsedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Referral code used event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}, UsageCount: {UsageCount}, MaxUsage: {MaxUsage}",
            notification.ReferralCodeId, notification.UserId, notification.Code, notification.UsageCount, notification.MaxUsage);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Usage limit kontrolü
        // - Notification gönderimi (milestone achievements)

        await Task.CompletedTask;
    }
}
