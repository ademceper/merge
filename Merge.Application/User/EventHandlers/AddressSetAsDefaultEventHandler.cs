using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

/// <summary>
/// Address Set As Default Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class AddressSetAsDefaultEventHandler : INotificationHandler<AddressSetAsDefaultEvent>
{
    private readonly ILogger<AddressSetAsDefaultEventHandler> _logger;

    public AddressSetAsDefaultEventHandler(ILogger<AddressSetAsDefaultEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AddressSetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Address set as default event received. AddressId: {AddressId}, UserId: {UserId}",
            notification.AddressId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (default address change metrics)
        // - Cache invalidation (user addresses cache, user preferences cache)
        // - External system integration (address preference update)
        // - Notification gönderimi (default address değişti bildirimi)

        await Task.CompletedTask;
    }
}
