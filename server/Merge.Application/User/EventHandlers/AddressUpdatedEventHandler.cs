using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddressUpdatedEventHandler(ILogger<AddressUpdatedEventHandler> logger) : INotificationHandler<AddressUpdatedEvent>
{

    public async Task Handle(AddressUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

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
