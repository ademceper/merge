using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Points Deducted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class PointsDeductedEventHandler : INotificationHandler<PointsDeductedEvent>
{
    private readonly ILogger<PointsDeductedEventHandler> _logger;

    public PointsDeductedEventHandler(ILogger<PointsDeductedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PointsDeductedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Points deducted event received. AccountId: {AccountId}, UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}, Reason: {Reason}",
            notification.AccountId, notification.UserId, notification.Points, notification.NewBalance, notification.Reason);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (points redeemed)

        await Task.CompletedTask;
    }
}
