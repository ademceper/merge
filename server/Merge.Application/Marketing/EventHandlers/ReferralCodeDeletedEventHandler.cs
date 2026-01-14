using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// ReferralCode Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class ReferralCodeDeletedEventHandler(ILogger<ReferralCodeDeletedEventHandler> logger) : INotificationHandler<ReferralCodeDeletedEvent>
{
    public async Task Handle(ReferralCodeDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "ReferralCode deleted event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}",
            notification.ReferralCodeId, notification.UserId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}