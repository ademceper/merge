using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddressCreatedEventHandler : INotificationHandler<AddressCreatedEvent>
{
    private readonly ILogger<AddressCreatedEventHandler> _logger;

    public AddressCreatedEventHandler(ILogger<AddressCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AddressCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation(
            "Address created event received. AddressId: {AddressId}, UserId: {UserId}, City: {City}, Country: {Country}, IsDefault: {IsDefault}",
            notification.AddressId, notification.UserId, notification.City, notification.Country, notification.IsDefault);

                // - Analytics tracking (address creation metrics)
        // - Cache invalidation (user addresses cache)
        // - External system integration (address validation service)
        // - Notification gönderimi (eğer default address ise)

        await Task.CompletedTask;
    }
}
