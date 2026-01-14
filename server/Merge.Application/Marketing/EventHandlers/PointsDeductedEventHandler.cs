using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Points Deducted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class PointsDeductedEventHandler(ILogger<PointsDeductedEventHandler> logger) : INotificationHandler<PointsDeductedEvent>
{
    public async Task Handle(PointsDeductedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Points deducted event received. AccountId: {AccountId}, UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}, Reason: {Reason}",
            notification.AccountId, notification.UserId, notification.Points, notification.NewBalance, notification.Reason);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (points redeemed)

        await Task.CompletedTask;
    }
}
