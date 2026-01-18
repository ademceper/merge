using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

public class AddressDeletedEventHandler(ILogger<AddressDeletedEventHandler> logger) : INotificationHandler<AddressDeletedEvent>
{

    public async Task Handle(AddressDeletedEvent notification, CancellationToken cancellationToken)
    {

        logger.LogInformation(
            "Address deleted event received. AddressId: {AddressId}, UserId: {UserId}",
            notification.AddressId, notification.UserId);

                // - Analytics tracking (address deletion metrics)
        // - Cache invalidation (user addresses cache)
        // - External system integration (address cleanup service)
        // - Order shipping address update (eğer bu address kullanılıyorsa)

        await Task.CompletedTask;
    }
}
