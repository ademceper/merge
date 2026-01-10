using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// Tier Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class TierUpdatedEventHandler : INotificationHandler<TierUpdatedEvent>
{
    private readonly ILogger<TierUpdatedEventHandler> _logger;

    public TierUpdatedEventHandler(ILogger<TierUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TierUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Tier updated event received. AccountId: {AccountId}, UserId: {UserId}, NewTierId: {NewTierId}",
            notification.AccountId, notification.UserId, notification.NewTierId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Tier upgrade email gönderimi
        // - Benefits notification
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
