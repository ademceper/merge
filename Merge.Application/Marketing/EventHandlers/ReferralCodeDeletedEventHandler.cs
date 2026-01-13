using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// ReferralCode Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class ReferralCodeDeletedEventHandler : INotificationHandler<ReferralCodeDeletedEvent>
{
    private readonly ILogger<ReferralCodeDeletedEventHandler> _logger;

    public ReferralCodeDeletedEventHandler(ILogger<ReferralCodeDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ReferralCodeDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "ReferralCode deleted event received. ReferralCodeId: {ReferralCodeId}, UserId: {UserId}, Code: {Code}",
            notification.ReferralCodeId, notification.UserId, notification.Code);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
