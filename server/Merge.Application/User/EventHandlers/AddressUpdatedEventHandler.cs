using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

public class AddressUpdatedEventHandler(ILogger<AddressUpdatedEventHandler> logger) : INotificationHandler<AddressUpdatedEvent>
{

    public async Task Handle(AddressUpdatedEvent notification, CancellationToken cancellationToken)
    {

        logger.LogInformation(
            "Address updated event received. AddressId: {AddressId}, UserId: {UserId}",
            notification.AddressId, notification.UserId);

                // - Analytics tracking (address update metrics)
        // - Cache invalidation (user addresses cache)
        // - External system integration (address validation service)
        // - Order shipping address update (eğer bu address kullanılıyorsa)

        await Task.CompletedTask;
    }
}
