using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

public class AddressSetAsDefaultEventHandler(ILogger<AddressSetAsDefaultEventHandler> logger) : INotificationHandler<AddressSetAsDefaultEvent>
{

    public async Task Handle(AddressSetAsDefaultEvent notification, CancellationToken cancellationToken)
    {

        logger.LogInformation(
            "Address set as default event received. AddressId: {AddressId}, UserId: {UserId}",
            notification.AddressId, notification.UserId);

                // - Analytics tracking (default address change metrics)
        // - Cache invalidation (user addresses cache, user preferences cache)
        // - External system integration (address preference update)
        // - Notification gönderimi (default address değişti bildirimi)

        await Task.CompletedTask;
    }
}
