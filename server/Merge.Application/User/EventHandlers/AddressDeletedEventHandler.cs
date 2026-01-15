using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddressDeletedEventHandler : INotificationHandler<AddressDeletedEvent>
{
    private readonly ILogger<AddressDeletedEventHandler> _logger;

    public AddressDeletedEventHandler(ILogger<AddressDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AddressDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation(
            "Address deleted event received. AddressId: {AddressId}, UserId: {UserId}",
            notification.AddressId, notification.UserId);

                // - Analytics tracking (address deletion metrics)
        // - Cache invalidation (user addresses cache)
        // - External system integration (address cleanup service)
        // - Order shipping address update (eğer bu address kullanılıyorsa)

        await Task.CompletedTask;
    }
}
